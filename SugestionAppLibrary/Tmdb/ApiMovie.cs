using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using SugestionAppLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMDbLib.Client;
using TMDbLib.Objects.Configuration;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.People;
using TMDbLib.Objects.Search;
using TMDbLib.Objects.Reviews;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using TMDbLib.Objects.Find;
using TMDbLib.Objects.Languages;

namespace SugestionAppLibrary.Tmdb;

public class ApiMovie
{
    private readonly IMovieDbData _movies;
    private readonly IMemoryCache _cacheConfiguration;
    private readonly TMDbClient _client;
    private const string cacheName = "MovieTMDB";
    private IConfiguration _configuration;

    public ApiMovie(IMovieDbData movieData, IMemoryCache cache, IConfiguration configuration)
    {
        _movies = movieData;
        _cacheConfiguration = cache;
        _configuration = configuration;
        _client = new TMDbClient(_configuration.GetValue<string>("TMDBApiKey:Value"));
    }

    public async Task<int> getTmdbIdByImdbId(string imdbId)
    {

        //Get the TMDB Id, posterpath e overview, using the ImdbId in the database. After getting the id, save on database to avoid new api call

        FindContainer? movieTmdb = _client.FindAsync(TMDbLib.Objects.Find.FindExternalSource.Imdb, imdbId).Result;
        var moviedb = await _movies.GetMovieByImdbId(imdbId);

        if (moviedb is not null && movieTmdb is not null && movieTmdb.MovieResults.Count > 0)
        {
            var movieFromApi = movieTmdb.MovieResults.FirstOrDefault();
            moviedb.TmdbId = movieFromApi.Id;
            moviedb.Overview = movieFromApi.Overview;
            moviedb.PosterPath = movieFromApi.PosterPath;
            moviedb.VoteAverage = movieFromApi.VoteAverage;
            moviedb.VoteCount = movieFromApi.VoteCount;
            moviedb.ReleaseDate = movieFromApi.ReleaseDate?.ToString("yyyy-MM-dd");
            moviedb.Popularity = movieFromApi.Popularity;
            moviedb.OriginalLanguage = movieFromApi.OriginalLanguage;
            await _movies.UpdateMovie(moviedb);
            return moviedb.TmdbId;
        }

        return 0;

    }

    public async Task<Movie> GetMovie(int movieId)
    {

        return await _client.GetMovieAsync(movieId, MovieMethods.Keywords | MovieMethods.Credits | MovieMethods.Images | MovieMethods.Recommendations | MovieMethods.Videos | MovieMethods.WatchProviders);

    }

    public async Task<APIConfiguration> GetConfiguration()
    {
        var output = _cacheConfiguration.Get<APIConfiguration>("ApiConfiguration");

        if (output is null)
        {
            output = await _client.GetAPIConfiguration();

            _cacheConfiguration.Set("ApiConfiguration", output, TimeSpan.FromDays(1));
        }
        return output;
    }

    public async Task<List<Language>> GetLanguages()
    {
        var output = _cacheConfiguration.Get<List<Language>>("LanguagesIso639");

        if (output is null)
        {
            output = await _client.GetLanguagesAsync();
            var languagesAvailable = await GetAllLanguagesAvailable();
            output = output.Where(f => languagesAvailable.Contains(f.Iso_639_1)).ToList();

            _cacheConfiguration.Set("LanguagesIso639", output, TimeSpan.FromDays(1));
        }
        return output;
    }

    private async Task<HashSet<string>> GetAllLanguagesAvailable()
    {
        var movies = await _movies.GetAllMovies();

        //Get all Original Languages on database

        var distinctLanguage = movies
        .GroupBy(p => p.OriginalLanguage)
        .Select(g => g.First())
        .ToList();

        var hashset = new HashSet<string>();
        foreach (var item in distinctLanguage)
        {

            hashset.Add(item.OriginalLanguage.Trim());

        }

        return hashset;

    }
}



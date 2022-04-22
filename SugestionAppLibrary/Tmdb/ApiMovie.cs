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

namespace SugestionAppLibrary.Tmdb;

public class ApiMovie
{
    private readonly IMovieDbData _movies;

    public ApiMovie(IMovieDbData movieData)
    {
        _movies = movieData;
    }
    public async Task<int> getTmdbIdByImdbId(string imdbId)
    {

        //Get the TMDB Id using the ImdbId in the database. After getting the id, save on database to avoid new api call

        TMDbClient client = new TMDbClient("6ef02e34ad385ea52b42eaa59bbd3638");
        //Movie movie = client.GetMovieAsync(47964).Result;
        var movieTmdb = client.FindAsync(TMDbLib.Objects.Find.FindExternalSource.Imdb, imdbId).Result;
        var moviedb = await _movies.GetMovieByImdbId(imdbId);

        if (moviedb is not null && movieTmdb is not null && movieTmdb.MovieResults.FirstOrDefault().Id > 0)
        {
            moviedb.TmdbId = movieTmdb.MovieResults.FirstOrDefault().Id;
            await _movies.UpdateMovie(moviedb);
            return moviedb.TmdbId;
        }

        return 0;

    }

    public async Task<Movie> GetMovie(int movieId)
    {

        TMDbClient client = new TMDbClient("6ef02e34ad385ea52b42eaa59bbd3638");
        MovieMethods movieMethods = MovieMethods.Keywords;
        return await client.GetMovieAsync(movieId, movieMethods );

    }

    public async Task<APIConfiguration> GetConfiguration()
    {

        TMDbClient client = new TMDbClient("6ef02e34ad385ea52b42eaa59bbd3638");
        var retorno = await client.GetAPIConfiguration();
        return retorno;
    }

    public string GetUrlTrailer(int movieId)
    {

        //Get the TMDB Id using the ImdbId in the database. After getting the id, save on database to avoid new api call

        TMDbClient client = new TMDbClient("6ef02e34ad385ea52b42eaa59bbd3638");
        var videosMovie = client.GetMovieVideosAsync(movieId);

        if (videosMovie is null || videosMovie.Result.Results.Count == 0)
            return "";

        var firstVideo = videosMovie.Result.Results.Where(f => f.Site == "YouTube").FirstOrDefault();
        return "https://www.youtube.com/embed/" + firstVideo.Key;

    }

    public async Task<Credits> GetCredit(int movieId)
    {

        TMDbClient client = new TMDbClient("6ef02e34ad385ea52b42eaa59bbd3638");
        return await client.GetMovieCreditsAsync(movieId);

    }

    public async Task<Person> GetPerson(int personId)
    {

        TMDbClient client = new TMDbClient("6ef02e34ad385ea52b42eaa59bbd3638");
        return await client.GetPersonAsync(personId);

    }

    public async Task<List<ImageData>> GetImageGallery(int movieId)
    {

        TMDbClient client = new TMDbClient("6ef02e34ad385ea52b42eaa59bbd3638");
        var gallery = await client.GetMovieImagesAsync(movieId);
        return gallery.Backdrops;

    }

    public async Task<SearchContainer<SearchMovie>> GetRelatedMovies(int movieId)
    {

        TMDbClient client = new TMDbClient("6ef02e34ad385ea52b42eaa59bbd3638");
        return await client.GetMovieRecommendationsAsync(movieId);
      

    }

    public async Task<SearchContainerWithId<ReviewBase>> GetReview(int movieId)
    {

        TMDbClient client = new TMDbClient("6ef02e34ad385ea52b42eaa59bbd3638");
        return await client.GetMovieReviewsAsync(movieId);

    }

    //public async static Task<string> GetCreditDetail(int movieId)
    //{
    //	try
    //	{
    //		var client = new RestClient("https://api.themoviedb.org/3/movie/28/credits?api_key=6ef02e34ad385ea52b42eaa59bbd3638&language=en-USappend_to_response=images")
    //		{

    //		};

    //		var request = new RestRequest("find/tt1392190", Method.Get);
    //		//request.AddQueryParameter("api_key", "6ef02e34ad385ea52b42eaa59bbd3638");
    //		//request.AddQueryParameter("language", "en-US");
    //		//request.AddQueryParameter("external_source", "imdb_id");
    //		var response = await client.GetAsync<RootMovie>(request);

    //		_json = response.ToJson();
    //		return response.ToJson();
    //	}
    //	catch (Exception ex)
    //	{
    //		return "Erro";
    //	}



    //}
}

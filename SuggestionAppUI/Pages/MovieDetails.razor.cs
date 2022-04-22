using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SuggestionAppUI.Shared;



namespace SuggestionAppUI.Pages;

public partial class MovieDetails
{
    [Parameter]
    public string? id { get; set; }

    bool isLoading = true;
    bool _firstRender = true;

    private MovieDbModel? movieDb;
    private Movie? movie;

    private APIConfiguration apiConfiguration;
    protected CastCrew castCrewComponent;
    protected MovieGallery movieGalleryComponent;
    protected RelatedMovies relatedMoviesComponent;
    protected MovieLists movieListComponent;


    //Variables of Movie to Render
    private string movieId = "";
    private string movieHeader = "";
    private string moviePoster = "images/uploads/poster_ph.jpg";
    private string movieName = "";
    private string movieYear = "";
    private string moviePhrase = "";
    private string movieVoteAvarage = "";
    private string movieVoteCount = "";
    private string movieOverview = "";
    private string movieGenres = "";
    private string movieReleaseDate = "";
    private string movieRunTime = "";
    private string movieTrailerLink = "";
    private string movieBudget = "";
    private string movieLanguage = "";
    private string movieRevenue = "";

    //Variables Cast and Crew to Render
    private string movieDirector = "";
    private string movieWriters = "";
    private string movieStars = "";

    //TabControl
    private Dictionary<string, string> tabLinks = new Dictionary<string, string>()
    {{"overview", "active"}, {"lists", ""}, {"cast", ""}, {"media", ""}, {"related", ""}};
    private Dictionary<string, string> tabStatus = new Dictionary<string, string>()
    {{"overview", "display:block;"}, {"lists", "display:none;"}, {"cast", "display:none;"}, {"media", "display:none;"}, {"related", "display:none;"}};


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {

        _firstRender = firstRender;

        if (firstRender)
        {

            apiConfiguration = await apiMovie.GetConfiguration();
            //if not id is provided, get a random movie

            int movieIdFromUrl = 0;

            if (id is null || !Int32.TryParse(id, out movieIdFromUrl))
            {
                await GetRandomMovie();
            }
            else
            {
                await GetMovie(movieIdFromUrl);
            }



        }
    }

    private async Task GetRandomMovie()
    {
        isLoading = true;

        movieDb = await movieData.GetRandonMovie();

        if (movieDb is null)
        {
            navManager.NavigateTo("/503");
        }

        if (movieDb.TmdbId == 0)
        {
            //Getting the id from tmdb and updating the database to prevent a new request
            movieDb.TmdbId = await apiMovie.getTmdbIdByImdbId(movieDb.Const);
        }

        //Getting MovieData from API TMDB
        movie = await apiMovie.GetMovie(movieDb.TmdbId);

        await UpdateComponents();
        await UpdateHooks();
       

    }

    private async Task GetMovie(int movieId)
    {
        isLoading = true;
        //Getting MovieData from API TMDB. Until now, only on the first load
        movie = await apiMovie.GetMovie(movieId);

        if (movie is null)
        {
            navManager.NavigateTo("/404");
        }

        await UpdateComponents();
        await UpdateHooks();

    }

    private async Task UpdateHooks()
    {
        //Updatating string hooks
        movieHeader = apiConfiguration.Images.SecureBaseUrl + "w1920_and_h800_multi_faces" + movie.BackdropPath;
        moviePoster = apiConfiguration.Images.SecureBaseUrl + "w500" + movie.PosterPath;
        movieName = movie.Title;
        movieYear = movie.ReleaseDate?.Year.ToString();
        movieId = movie.Id.ToString();
        moviePhrase = string.IsNullOrEmpty(movie.Tagline) ? "..." : movie.Tagline;
        movieVoteAvarage = movie.VoteAverage.ToString();
        movieVoteCount = movie.VoteCount.ToString();
        movieOverview = movie.Overview;
        movieGenres = string.Join(",", movie.Genres.Select(x => x.Name));
        movieReleaseDate = movie.ReleaseDate?.ToString("MMMM dd, yyyy");
        movieRunTime = movie.Runtime is null ? " " : movie.Runtime?.ToString();
        movieBudget = movie.Budget == 0 ? "-" : movie.Budget.ToString("C");
        movieRevenue = movie.Revenue == 0 ? "-" : movie.Revenue.ToString("C");
        movieLanguage = movie.OriginalLanguage;
        //Getting movie trailer
        movieTrailerLink = GetUrlTrailer(movie.Videos.Results);

        //Credit
        
        if (movie.Credits is not null)
        {
            movieDirector = string.Join(",", movie.Credits.Crew.Where(d => d.Job == "Director").ToList().Select(x => x.Name));
            movieWriters = string.Join(",", movie.Credits.Crew.Where(d => d.Department == "Writing").ToList().Select(x => x.Name));
            movieStars = string.Join(",", movie.Credits.Cast.Take(8).Select(x => x.Name));
        }
        isLoading = false;
        await jsRunTime.InvokeVoidAsync("ChangeUrl", "/Movie/" + movie.Id);
        StateHasChanged();

    }

    private async Task UpdateComponents()
    {
        if (!_firstRender && movie is not null)
        {
            //Only request if is not the first render (user click in a new movie)
            await castCrewComponent.LoadCredit(movie.Credits);
            await movieGalleryComponent.LoadGallery(movie.Images.Backdrops);
            await relatedMoviesComponent.LoadRelatedMovies(movie.Recommendations.Results);
            await movieListComponent.LoadListWithMovie(movie.ImdbId);
        }

    }

    private void ChangeTabStatus(string tabName)
    {
        foreach (var key in tabLinks.Keys.ToList())
        {
            if (key == tabName)
            {
                tabLinks[key] = "active";
                tabStatus[key] = "display:block;";
            }
            else
            {
                tabLinks[key] = "";
                tabStatus[key] = "display:none;";
            }
        }

        jsRunTime.InvokeVoidAsync("scrollToTab", tabName);
    }

    private string GetInitials(string Name)
    {
        var arrayName = Name.Split(' ');
        //If only has one name
        if (arrayName.Count() == 1)
        {
            return arrayName[0].Substring(0, 2);
        }
        else
        {
            return arrayName[0].Substring(0, 1) + arrayName[arrayName.Count() - 1].Substring(0, 1);
        }
    }

    private void CloseIframeYoutube()
    {
        jsRunTime.InvokeVoidAsync("CloseIframeYoutube");
    }

    public string GetUrlTrailer(List<Video> videosMovie)
    {

        if (videosMovie is null || videosMovie.Count == 0)
        {
            return "";
        }
      

        Video? firstVideo = videosMovie.Where(f => f.Site == "YouTube").FirstOrDefault();

        if (firstVideo is null)
        {
            return "";
        }

        return "https://www.youtube.com/embed/" + firstVideo.Key;

    }

}

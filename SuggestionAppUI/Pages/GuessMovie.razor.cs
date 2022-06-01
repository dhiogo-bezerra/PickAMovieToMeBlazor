using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MozifAppUI.Shared;
using TMDbLib.Objects.Languages;

namespace MozifAppUI.Pages;

public partial class GuessMovie
{
    private MovieDbModel? movieDb;
    private Movie? movie;
    private List<Language> iso339Languages;
    private APIConfiguration apiConfiguration;
    private ImageData? backdrop = null;

    private string movieYear = "";
    private string movieGenres = "";
    private string movieRunTime = "";
    private string movieVoteAverage = "";
    private string movieOriginalLanguage = "";
    private string tagline = "";
    private string movieWriters = "";
    private string movieActor = "";
    private string movieKeywords = "";
    private string movieDirector = "";
    private string movieTitle = "";

    private string fieldGuess = "";

    private string btnStatus = "";
    private string newGuessStatus = "hidden";

    private int pontuation = 10;
    private string classPontuation = "green";
    private string message = "";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {

        if (firstRender)
        {
            iso339Languages = await apiMovie.GetLanguages();
            apiConfiguration = await apiMovie.GetConfiguration();
            await GetRandomMovie();
            await jsRunTime.InvokeAsync<IJSObjectReference>("import", "/js/custom.js");

        }
    }

    private async Task GetRandomMovie()
    {

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

        //First clues
        movieYear = movie.ReleaseDate?.Year.ToString();
        movieGenres = string.Join(",", movie.Genres.Select(x => x.Name));
        StateHasChanged();

    }

    private void SubmitGuess()
    {
        if (string.IsNullOrWhiteSpace(fieldGuess.Trim()))
        {
            message = "Enter a movie name.";
            return;
        }
        if (fieldGuess.Trim().Equals(movie.Title, StringComparison.InvariantCultureIgnoreCase))
        {
            //Right Guess
            message = "You guessed it right!";
            RightGuesss();
            btnStatus = "hidden";
            newGuessStatus = "";
        }
        else
        {
            message = "Wrong guess.";
            GetAnotherClue();
            fieldGuess = "";
        }

        StateHasChanged();

    }

    private void RightGuesss()
    {

        movieTitle = movie.Title;

        movieVoteAverage = movie.VoteAverage.ToString();

        movieRunTime = movie.Runtime is null ? " " : movie.Runtime?.ToString() + " mins";

        movieOriginalLanguage = iso339Languages.First(l => l.Iso_639_1 == movie.OriginalLanguage).EnglishName;

        tagline = movie.Tagline;

        if (movie.Credits is not null)
        {
            movieWriters = string.Join(",", movie.Credits.Crew.Where(d => d.Department == "Writing").ToList().Select(x => x.Name));
        }

        if (movie.Credits is not null)
        {
            movieActor = string.Join(",", movie.Credits.Cast.Take(4).Select(x => x.Name));
        }

        if (movie.Keywords is not null)
        {
            movieKeywords = string.Join(",", movie.Keywords.Keywords.Take(4).Select(x => x.Name));
        }

        if (movie.Credits is not null)
        {
            movieDirector = string.Join(",", movie.Credits.Crew.Where(d => d.Job == "Director").ToList().Select(x => x.Name));
        };

        if (movie.Images.Backdrops is not null)
        {
            backdrop = movie.Images.Backdrops.Random();
        }

    }

    private void GetAnotherClue()
    {
        message = "";

        switch (pontuation)
        {
            case 10:
                movieVoteAverage = movie.VoteAverage.ToString();
                break;
            case 9:
                movieRunTime = movie.Runtime is null ? " " : movie.Runtime?.ToString();
                break;
            case 8:
                movieOriginalLanguage = iso339Languages.First(l => l.Iso_639_1 == movie.OriginalLanguage).EnglishName;
                break;
            case 7:
                tagline = movie.Tagline;
                classPontuation = "yellow";
                break;
            case 6:
                if (movie.Credits is not null)
                {
                    movieWriters = string.Join(",", movie.Credits.Crew.Where(d => d.Department == "Writing").ToList().Select(x => x.Name));
                }
                break;
            case 5:
                if (movie.Credits is not null)
                {
                    movieActor = string.Join(",", movie.Credits.Cast.Take(8).Select(x => x.Name));
                }
                classPontuation = "red";
                break;
            case 4:
                if (movie.Keywords is not null)
                {
                    movieKeywords = string.Join(",", movie.Keywords.Keywords.Select(x => x.Name));
                }
                break;
            case 3:
                if (movie.Credits is not null)
                {
                    movieDirector = string.Join(",", movie.Credits.Crew.Where(d => d.Job == "Director").ToList().Select(x => x.Name));
                };
                break;
            case 2:
                if (movie.Images.Backdrops is not null)
                {
                    backdrop = movie.Images.Backdrops.Random();
                }
                break;
            case 1:
                movieTitle = movie.Title;
                break;

        }



        if (pontuation > 1)
        {
            pontuation--;
        }
        else
        {
            pontuation--;
            btnStatus = "hidden";
            newGuessStatus = "";
        }

        StateHasChanged();
    }

    private async Task NewGuess()
    {
        pontuation = 10;
        classPontuation = "green";
        fieldGuess = "";
        message = "";

        movieYear = "";
        movieGenres = "";
        movieRunTime = "";
        movieVoteAverage = "";
        movieOriginalLanguage = "";
        tagline = "";
        movieWriters = "";
        movieActor = "";
        movieKeywords = "";
        movieDirector = "";
        movieTitle = "";
        backdrop = null;

        newGuessStatus = "hidden";
        btnStatus = "";

        await GetRandomMovie();
        StateHasChanged();
    }
}

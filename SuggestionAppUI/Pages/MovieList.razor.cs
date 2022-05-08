using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using SuggestionAppUI;
using SuggestionAppUI.Shared;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using TMDbLib.Objects.Languages;

namespace SuggestionAppUI.Pages
{
    public partial class MovieList
    {
        //Control if the loading div is displayed
        bool isLoading = true;
        private List<MovieDbModel> _movieList;
        private List<MovieListModel> _lists;
        private List<GenreModel> _genres;


        private int _page = 0;
        private int _totalPages = 0;
        private int _itemPerPage = 5;
        private APIConfiguration _apiConfiguration;
        private List<Language> iso339Languages;

        private int _movieCount = 0;
        private int pagesBefore = 0;
        private int pagesAfter = 0;
        private string FilterTitle { get; set; } = "";
        private string FilterDirector { get; set; } = "";
        private string FilterFromYear { get; set; } = "";
        private string FilterToYear { get; set; } = "";
        private string FilterRange { get; set; } = "";
        private string FilterLanguage { get; set; } = "";
        private string[] SelectedGenre { get; set; } = Array.Empty<string>();
        private string[] SelectLists { get; set; } = Array.Empty<string>();


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                isLoading = true;

                //Loading Js multiselect ui dropdown
                await jsRunTime.InvokeVoidAsync("MultiselectInit");

                _apiConfiguration = await apiMovie.GetConfiguration();
                //_movieList = await movieDbData.GetAllMovies();
                _genres = await genreData.GetAllGenre();
                _genres = _genres.OrderBy(f => f.Name).ToList();
                _lists = await movieListData.GetAllLists();
                iso339Languages = await apiMovie.GetLanguages();


                await LoadFilterState();
                FilterSubmit();
                UpdatePage();
                isLoading = false;
                StateHasChanged();

                await jsRunTime.InvokeAsync<IJSObjectReference>("import", "/js/custom.js");
            }
        }

        private async Task LoadFilterState()
        {
            var stringResults = await localStorage.GetAsync<string>(nameof(FilterTitle));
            FilterTitle = stringResults.Success ? stringResults.Value : "";

            stringResults = await localStorage.GetAsync<string>(nameof(FilterDirector));
            FilterDirector = stringResults.Success ? stringResults.Value : "";

            stringResults = await localStorage.GetAsync<string>(nameof(FilterLanguage));
            FilterLanguage = stringResults.Success ? stringResults.Value : "";

            stringResults = await localStorage.GetAsync<string>(nameof(FilterFromYear));
            FilterFromYear = stringResults.Success ? stringResults.Value : "";

            stringResults = await localStorage.GetAsync<string>(nameof(FilterToYear));
            FilterToYear = stringResults.Success ? stringResults.Value : "";

            stringResults = await localStorage.GetAsync<string>(nameof(FilterToYear));
            FilterToYear = stringResults.Success ? stringResults.Value : "";

            stringResults = await localStorage.GetAsync<string>(nameof(FilterRange));
            FilterRange = stringResults.Success ? stringResults.Value : "";

            var stringArrayResult = await localStorage.GetAsync<string[]>(nameof(SelectedGenre));
            SelectedGenre = stringArrayResult.Success ? stringArrayResult.Value : Array.Empty<string>();

            stringArrayResult = await localStorage.GetAsync<string[]>(nameof(SelectLists));
            SelectLists = stringArrayResult.Success ? stringArrayResult.Value : Array.Empty<string>();
        }

        private async Task SaveFilterState()
        {
            //Save filter on localstorage
            await localStorage.SetAsync(nameof(FilterTitle), FilterTitle);
            await localStorage.SetAsync(nameof(FilterDirector), FilterDirector);
            await localStorage.SetAsync(nameof(FilterLanguage), FilterLanguage);
            await localStorage.SetAsync(nameof(FilterFromYear), FilterFromYear);
            await localStorage.SetAsync(nameof(FilterToYear), FilterToYear);
            await localStorage.SetAsync(nameof(FilterRange), FilterRange);
            await localStorage.SetAsync(nameof(SelectedGenre), SelectedGenre);
            await localStorage.SetAsync(nameof(SelectLists), SelectLists);
        }

        private async Task DeleteFilterState()
        {
            //Save filter on localstorage
            await localStorage.DeleteAsync(nameof(FilterTitle));
            await localStorage.DeleteAsync(nameof(FilterDirector));
            await localStorage.DeleteAsync(nameof(FilterLanguage));
            await localStorage.DeleteAsync(nameof(FilterFromYear));
            await localStorage.DeleteAsync(nameof(FilterToYear));
            await localStorage.DeleteAsync(nameof(FilterRange));
            await localStorage.DeleteAsync(nameof(SelectedGenre));
            await localStorage.DeleteAsync(nameof(SelectLists));
        }

        protected void UpdatePage()
        {
            _totalPages = (_movieCount / _itemPerPage);
            //if has fractional remainder, add 1
            if (_movieCount % _itemPerPage > 0)
            {
                _totalPages++;
            }

            pagesBefore = _page - 2;
            pagesAfter = _page + 3;
            if (pagesBefore < 1)
            {
                pagesBefore = 1;
            }

            if (pagesAfter > _totalPages)
            {
                pagesAfter = _totalPages;
            }
        }

        public void LoadMovie(string MovieId)
        {
            navManager.NavigateTo("/Movie/" + MovieId);
        }

        protected async Task NavigatePage(int page)
        {
            _page = --page;
            UpdatePage();
            StateHasChanged();
            await jsRunTime.InvokeVoidAsync("OnScrollEvent");
        }

        protected void UpdateMoviePerPage(ChangeEventArgs e)
        {
            if (e.Value is not null)
            {
                _itemPerPage = int.Parse(e.Value.ToString());
                UpdatePage();
                StateHasChanged();
            }
        }

        protected void ResetPagination()
        {
            _page = 0;
            _movieCount = _movieList.Count;
            UpdatePage();
        }

        protected async void FilterSubmit()
        {
            isLoading = true;

            var output = await movieDbData.GetAllMovies();

            //Filter by Title.
            if (FilterTitle.Length > 3)
            {
                output = output.Where(f => f.Title.Contains(FilterTitle, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            //Filter by Director.
            if (FilterDirector.Length > 3)
            {
                output = output.Where(f => f.Directors.Contains(FilterDirector, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            //Filter by Language.
            if (!string.IsNullOrEmpty(FilterLanguage))
            {
                output = output.Where(f => f.OriginalLanguage.Equals(FilterLanguage)).ToList();
            }


            //Filter by Year.
            if (FilterFromYear.Length > 3 && FilterToYear.Length > 3)
            {
                output = output.Where(f => f.Year >= Convert.ToInt16(FilterFromYear) && f.Year <= Convert.ToInt16(FilterToYear)).ToList();
            }

            //Filter by Genre.
            if (SelectedGenre.Length > 0)
            {
                output = (output.Where(f => f.Genres.Split(',').ToList<string>().Any(x => SelectedGenre.Any(y => x.Contains(y)))).ToList());
            }

            //Filter by List.
            if (SelectLists.Length > 0)
            {
                output = output.Where(f => f.MemberOf.Any(x => SelectLists.Any(y => x.Id == y))).ToList();
            }

            //Filter by Range
            if (!string.IsNullOrEmpty(FilterRange))
            {
                output = FilterRange switch
                {
                    "0to3" => output.Where(f => f.VoteAverage <= Convert.ToDouble(3)).ToList(),
                    "3to6" => output.Where(f => f.VoteAverage > Convert.ToDouble(3) && f.VoteAverage <= Convert.ToDouble(6)).ToList(),
                    "6to9" => output.Where(f => f.VoteAverage > Convert.ToDouble(6) && f.VoteAverage <= Convert.ToDouble(9)).ToList(),
                    "9to10" => output.Where(f => f.VoteAverage > Convert.ToDouble(9)).ToList(),
                    _ => output,
                };
            }

            await SaveFilterState();


            _movieList = output;
            isLoading = false;
            ResetPagination();
            StateHasChanged();
        }

        protected async void FilterByDirectorName(string director)
        {
            var output = await movieDbData.GetAllMovies();
            output = output.Where(f => f.Directors.Contains(director, StringComparison.InvariantCultureIgnoreCase)).ToList();
            FilterDirector = director;
            ResetPagination();
        }

        protected async void Reset()
        {
            FilterTitle = "";
            FilterDirector = "";
            FilterFromYear = "";
            FilterToYear = "";
            FilterLanguage = "";
            SelectLists = Array.Empty<string>();
            SelectedGenre = Array.Empty<string>();
            _movieList = await movieDbData.GetAllMovies();
            await DeleteFilterState();

            ResetPagination();
            StateHasChanged();

            //Reset select dropdown ui with JS
            await jsRunTime.InvokeVoidAsync("clearDropdownui");
        }

        protected void SortMovieList(ChangeEventArgs e)
        {
            isLoading = true;
            var output = e.Value.ToString() switch
            {
                "ratingDsc" => _movieList.OrderByDescending(f => f.VoteAverage).ToList(),
                "ratingAsc" => _movieList.OrderBy(f => f.VoteAverage).ToList(),
                "popularityDsc" => _movieList.OrderByDescending(f => f.VoteCount).ToList(),
                "popularityAsc" => _movieList.OrderBy(f => f.VoteCount).ToList(),
                "dateDsc" => _movieList.OrderByDescending(f => f.ReleaseDate).ToList(),
                "dateAsc" => _movieList.OrderBy(f => f.ReleaseDate).ToList(),
                "listTotalDesc" => _movieList.OrderByDescending(f => f.MemberOf.Count).ToList(),
                "listTotalAsc" => _movieList.OrderBy(f => f.MemberOf.Count).ToList(),
                _ => _movieList,
            };
            _movieList = output;
            ResetPagination();
            isLoading = false;
            StateHasChanged();
        }

        protected void SelectedListChange(ChangeEventArgs e) => SelectLists = (string[])e.Value;

        protected void SelectedGenreChange(ChangeEventArgs e) => SelectedGenre = (string[])e.Value;

        protected void SelectedLanguageChange(ChangeEventArgs e)
        {
            var selectedLanguage = iso339Languages.FirstOrDefault(f => f.EnglishName.Equals(e.Value.ToString()));
            FilterLanguage = selectedLanguage is null ? "" : selectedLanguage.Iso_639_1;
        }

        private async Task YearInitInput(string searchInput)
        {
            FilterFromYear = searchInput;


            if (FilterFromYear.Length < 4)
            {
                return;
            }


            if (!string.IsNullOrWhiteSpace(FilterToYear) && Int16.TryParse(FilterToYear, out short toYear) && Int16.TryParse(FilterFromYear, out short fromYear))
            {
                FilterToYear = FilterToYear;
            }
        }
    }
}
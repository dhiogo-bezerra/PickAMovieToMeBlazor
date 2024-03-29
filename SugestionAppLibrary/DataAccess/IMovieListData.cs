﻿
namespace MozifAppLibrary.DataAccess
{
    public interface IMovieListData
    {
        Task CreateList(MovieListModel list);
        Task<List<MovieListModel>> GetAllLists();
    }
}
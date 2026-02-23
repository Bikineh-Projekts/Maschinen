using Microsoft.EntityFrameworkCore;

namespace MaschinenDataein.Models.PaginatedModel
{
        public class PaginatedList<T> : List<T>
        {
            public int PageIndex { get; private set; }
            public int TotalPages { get; private set; }

            public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
            {
                PageIndex = pageIndex;
                TotalPages = (int)Math.Ceiling(count / (double)pageSize);

                this.AddRange(items);
            }

            public bool HasPreviousPage => PageIndex > 1;

            public bool HasNextPage => PageIndex < TotalPages;

            public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
            {
                var count = await source.CountAsync();
                var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
                return new PaginatedList<T>(items, count, pageIndex, pageSize);
            }
        }

        public class PaginatedListItem
        {
            public PaginatedListItem(int pageNumber, int pageCount, bool isFirstPage, bool isLastPage)
            {
                if (pageCount == 0)
                {
                    PageNumber = pageCount;
                    IsFirstPage = true;
                    IsLastPage = true;
                }
                else
                {
                    PageNumber = pageNumber;
                    IsFirstPage = isFirstPage;
                    IsLastPage = isLastPage;
                }
                PageCount = pageCount;

            }
        public PaginatedListItem(int pageNumber, int pageCount, bool isFirstPage, bool isLastPage, string functionName)
        {
            if (pageCount == 0)
            {
                PageNumber = pageCount;
                IsFirstPage = true;
                IsLastPage = true;
            }
            else
            {
                PageNumber = pageNumber;
                IsFirstPage = isFirstPage;
                IsLastPage = isLastPage;
            }
            PageCount = pageCount;
            FunctionName = functionName;
            Id = 0;

        }

        public PaginatedListItem(int pageNumber, int pageCount, bool isFirstPage, bool isLastPage, string functionName, int id)
            {
                if (pageCount == 0)
                {
                    PageNumber = pageCount;
                    IsFirstPage = true;
                    IsLastPage = true;
                }
                else
                {
                    PageNumber = pageNumber;
                    IsFirstPage = isFirstPage;
                    IsLastPage = isLastPage;
                }
                PageCount = pageCount;
                FunctionName = functionName;
                Id = id;

            }
            public string FunctionName { get; set; }
            public int Id { get; set; }
            public int PageNumber { get; set; }
            public int PageCount { get; set; }
            public bool IsFirstPage { get; set; }
            public bool IsLastPage { get; set; }

        }

    }

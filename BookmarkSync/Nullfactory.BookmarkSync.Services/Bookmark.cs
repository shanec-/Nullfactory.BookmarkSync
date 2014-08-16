
using System.Collections;
using System;
using System.Collections.Generic;
namespace Nullfactory.BookmarkSync.Services
{
    public class Bookmark
    {
        public string Title { get; set; }
        public string Url { get; set; }
    }

    public class UrlComparer<T> : IEqualityComparer<T>
    {

        public bool Equals(T x, T y)
        {
            var left = x as Bookmark;
            var right = y as Bookmark;

            if (left == null || right == null)
                throw new InvalidOperationException(string.Format("Type of {0} Expected", typeof(Bookmark).ToString()));

            return (left.Title.Equals(right.Title));
        }

        public int GetHashCode(T obj)
        {
            var x = obj as Bookmark;
            if (x == null)
                throw new InvalidOperationException(string.Format("Type of {0} Expected", typeof(Bookmark).ToString()));

            return x.Title.GetHashCode();
        }
    }
}

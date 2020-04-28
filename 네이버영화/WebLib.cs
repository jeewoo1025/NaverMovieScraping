using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace 네이버영화
{
    class MovieDB
    {
        public string title = "";
        public string url = "";                 // url
        public string audRating = "";           // 관객 평점
        public string expRating = "";           // 전문가 평점
        public string netRating = "";           // 네티즌 평점
        public List<string> genre = new List<string>();
        public List<string> nation = new List<string>();
        public List<string> director = new List<string>();
        public List<string> actor = new List<string>();
        public List<string> releaseDate = new List<string>();
        public List<string> recommendMovies = new List<string>();

        public bool Is()
        {
            if(title != "" && url != "")
            {
                return true;
            }

            return false;
        }
    }

    class WebLib
    {
        static public string FindPoster(HtmlDocument doc)
        {
            string imageUrl = "";
            HtmlElement movie = doc.GetElementById("content");
            List<HtmlElement> itemList = GetElementsByTagAndClassName(movie, "div", "mv_info_area");

            int lineIndex = 0;
            foreach (HtmlElement item in itemList)
            {
                foreach(HtmlElement item2 in item.GetElementsByTagName("img"))
                {
                    if (lineIndex == 0)
                    {
                        imageUrl = item2.GetAttribute("src");
                    }
                }    
            }

            //int endIndex = imageUrl.IndexOf('?');
            //imageUrl.Substring(0, endIndex);
            return imageUrl;
        }

        static public Uri MakeSearchUrl(string title)
        {
            Uri url = new Uri("https://movie.naver.com/movie/search/result.nhn?query=" +
                title + "&section=all&ie=utf8");
            return url;
        }

        static public MovieDB MakeMovieDB(HtmlDocument doc)
        {
            List<MovieDB> dbs = new List<MovieDB>();
            HtmlElementCollection movieList = doc.GetElementsByTagName("dl");

            foreach (HtmlElement movieElement in movieList)
            {
                MovieDB db;
                if (MakeMovieDB(movieElement, out db))
                    dbs.Add(db);
            }

            return dbs[0];
        }

        static public List<HtmlElement> GetElementsByTagAndClassName(HtmlElement movie, string tag, string className)
        {
            List<HtmlElement> list = new List<HtmlElement>();

            HtmlElementCollection elmts = movie.GetElementsByTagName(tag);
            for(int i  = 0; i < elmts.Count; i++)
            {
                if(elmts[i].GetAttribute("className") == className)
                {
                    list.Add(elmts[i]);
                }
            }

            return list;
        }

        static public string CountRatings(HtmlElement star_score)
        {
            string text = "";
            HtmlElementCollection list_enum = star_score.GetElementsByTagName("em");
            
            foreach (HtmlElement em in list_enum)
                text += em.InnerText;

            return text;
        }

        static public void GetRatings(HtmlElement movie, MovieDB db)
        {
            List<HtmlElement> listRatings = GetElementsByTagAndClassName(movie, "div", "star_score");
            int lineIndex = 0;

            foreach(HtmlElement item in listRatings)
            {
                if (lineIndex == 0)
                    db.audRating = CountRatings(item);
                else if (lineIndex == 1)
                    db.expRating = CountRatings(item);

                lineIndex++;
            }

            HtmlElement netRatings = GetElementsByTagAndClassName(movie, "div", "star_score ")[0];
            db.netRating = CountRatings(netRatings);
        }

        public static void GetRecommendMovies(HtmlElement movie, MovieDB db)
        {
            HtmlElement listMovies = GetElementsByTagAndClassName(movie, "div", "link_movie type2")[0];

            int linelindex = 0;
            foreach(HtmlElement item in listMovies.GetElementsByTagName("ul"))
            {
                if(linelindex == 0)
                {
                    string text = item.InnerText;
                    text = text.Replace("\r\n", ",");
                    text = text.Replace(",,, ,", string.Empty);
                    text = text.Substring(4);

                    string[] movies = text.Split(',');
                    db.recommendMovies.AddRange(movies);
                }
            }
        }

        static public bool UpdateMovieDB(HtmlDocument doc, MovieDB db)
        {
            HtmlElement movie = doc.GetElementById("content");
            int lineIndex = 0;

            // 장르, 국가, 개봉일, 감독, 출연진
            foreach (HtmlElement item in movie.GetElementsByTagName("dd"))
            {
                if (lineIndex == 2)
                {
                    foreach (HtmlElement item2 in item.GetElementsByTagName("a"))
                    {

                        string text = item2.InnerText;
                        string href = item2.GetAttribute("href");

                        if (href.IndexOf("genre") > -1)
                            db.genre.Add(text);
                        else if (href.IndexOf("nation") > -1)
                            db.nation.Add(text);
                        else if (href.IndexOf("open") > -1)
                            db.releaseDate.Add(text);

                    }
                }
                else if(lineIndex == 3)
                {
                    string text = item.InnerText;
                    text = text.Replace(", ", ",");

                    string[] director = text.Split(',');
                    db.director.AddRange(director);
                }
                else if(lineIndex == 4)
                {
                    string text = item.InnerText;
                    text = text.Replace("\r\n", ",");
                    text = text.Replace(", ", ",");
                    text = text.Replace("더보기", string.Empty);

                    string[] actor = text.Split(',');
                    db.actor.AddRange(actor);
                }

                lineIndex++;
            }

            // 평점
            GetRatings(movie, db);

            // 유사한 영화들
            GetRecommendMovies(movie, db);

            return db.Is();
        }
       
        static public bool MakeMovieDB(HtmlElement movieElement, out MovieDB db)
        {
            db = new MovieDB();

            foreach (HtmlElement item in movieElement.GetElementsByTagName("dt"))
            {
                foreach (HtmlElement item2 in item.GetElementsByTagName("a"))
                {
                    db.title = item2.InnerText;
                    db.url = item2.GetAttribute("href");
                }
            }

            return db.Is();
        }

        static public void GetReview(HtmlDocument doc, List<string> list)
        {
            HtmlElement movie = doc.GetElementById("content");
            List<HtmlElement> listReview = GetElementsByTagAndClassName(movie, "div", "score_reple");
            int lineIndex = 0;

            foreach(HtmlElement item in listReview)
            {
                if(lineIndex <= 5)
                {
                    string text = item.InnerText;
                    text = text.Replace("신고", string.Empty);

                    list.Add(text);
                }
                lineIndex++;
            }
        }
    }
}

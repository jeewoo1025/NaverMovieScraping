using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

namespace 네이버영화
{
    enum WebBrowserStatus
    {
        INFO,
        MOVIE_FIND,
        POSTER_FIND,
        POSTER_DOWN,
        END
    }

    public partial class Form1 : Form
    {
        MovieDB db;
        List<string> listReview;
        WebBrowserStatus webStatus;

        public Form1()
        {
            InitializeComponent();

            listReview = new List<string>();
            webStatus = WebBrowserStatus.INFO;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void Search()
        {
            if (txtSearch.Text == "")
                return;

            webStatus = WebBrowserStatus.INFO;
            Uri url = WebLib.MakeSearchUrl(txtSearch.Text);
            webBrowser1.Navigate(url.AbsoluteUri);

            listReview.Clear();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            Search();
        }

        private string printList(List<string> tmp)
        {
            string str = tmp[0];
            for (int i = 1; i < tmp.Count; i++)
                str += ", " + tmp[i];

            return str;
        }

        private void Output(MovieDB db)
        {
            StringBuilder str = new StringBuilder();

            str.AppendLine("URL : " + db.url);
            str.AppendLine("제목 : " + db.title);
            str.AppendLine("개봉일 : " + printList(db.releaseDate));
            str.AppendLine("관람객 평점 : " + db.audRating);
            str.AppendLine("전문가 평점 : " + db.expRating);
            str.AppendLine("네티즌 평점 : " + db.netRating);
            str.AppendLine("장르 : " + printList(db.genre));
            str.AppendLine("국가 : " + printList(db.nation));
            str.AppendLine("감독 : " + printList(db.director));
            str.AppendLine("배우 : " + printList(db.actor));
            str.AppendLine("유사한 영화들 : " + printList(db.recommendMovies));

            txtBox.Text = str.ToString();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if(webStatus == WebBrowserStatus.INFO)
            {
                db = WebLib.MakeMovieDB(webBrowser1.Document);
                MovePage();
            }
            else if(webStatus == WebBrowserStatus.MOVIE_FIND)
            {
                WebLib.UpdateMovieDB(webBrowser1.Document, db);
                PrintReview();
                Poster();
                webStatus = WebBrowserStatus.END;
            }

            Output(db);
        }

        private void Poster()
        {
            string posterPageUrl = WebLib.FindPoster(webBrowser1.Document);
            pictureBox1.ImageLocation = posterPageUrl;
        }

        private void PrintReview()
        {
            StringBuilder str = new StringBuilder();

            WebLib.GetReview(webBrowser1.Document, listReview);
            for (int i = 0; i < listReview.Count; i++)
            {
                str.Append(listReview[i]);
                str.Append("\r\n\r\n");
            }

            txtReview.Text = str.ToString();
        }

        private void MovePage()
        {
            if (db.Is() == false)
                return;

            webStatus = WebBrowserStatus.MOVIE_FIND;
            webBrowser1.Navigate(db.url);
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
                Search();
        }
    }
}

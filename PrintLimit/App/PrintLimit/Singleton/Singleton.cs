using PrintLimit.Data.EF;
using PrintLimit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintLimit
{
    //Single Design Pattern
    public sealed class Singleton
    {
        private static string _paperSize = "";
        private static string _document = "";
        private static int _copies = 0;
        private static string _namePrinter = "";
        public static List<BanInViewModel> _lstSpooling = new List<BanInViewModel>();
        private static Singleton Instance = null;

        public string Document { get => _document; set => _document = value; }
        public string PaperSize { get => _paperSize; set => _paperSize = value; }
        public int Copies { get => _copies; set => _copies = value; }
        public string NamePrinter { get => _namePrinter; set => _namePrinter = value; }
        public List<BanInViewModel> ListSpooling { get => _lstSpooling; set => _lstSpooling = value; }

        public static Singleton GetInstance()
        {

            if (Instance == null)
            {
                Instance = new Singleton();
            }
            return Instance;
        }


        public void Refresh()
        {
            Document = "";
            PaperSize = "";
            Copies = 0;
            NamePrinter = "";
        }
    }
}

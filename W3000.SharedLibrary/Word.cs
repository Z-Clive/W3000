
using System;

namespace W3000.SharedLibrary
{
    public enum WordRating
    {
        Good,
        Soso,
        Poor,
    }
    
    /// <summary>
    /// Description of Word.
    /// </summary>
    public class Word
    {
        public string Eng { get; private set; }
        public string Chn { get; private set; }
        public int Rating { get; set; }
        
        public Word(string eng, string chn)
        {
            Eng = eng;
            Chn = chn;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace W3000.SharedLibrary
{
    /// <summary>
    /// Description of CourseManager.
    /// </summary>
    public class CourseManager
    {
        private List<Word> _words = new List<Word>();
        private List<Word> _backupWords;
        private int _currentPosition = -1;
        
        public Word CurrentWord{ get { return _words[_currentPosition]; } }
        
        public int Position { get { return _currentPosition; } }
        
        public int Count { get { return _words.Count; } }
        
        public CourseManager(StreamReader sr, bool isRandom)
        {
            try
            {
                var str = string.Empty;
                while (!string.IsNullOrEmpty(str = sr.ReadLine()))
                {
                    var strs = str.Split('\t');
                    if (strs.Length == 2)
                    {
                        _words.Add(new Word(strs[0],strs[1]));
                    }
                    else if (strs.Length == 3)
                    {
                        _words.Add(new Word(strs[0], strs[1])
                                   { Rating = int.Parse(strs[2]) });
                    }
				}

				if (_words.Count == 0)
				{
					throw new Exception("课程文件空！");
				}
            }
            catch (Exception ex)
            {
				throw new FormatException("课程文件无效，请更换并重试。", ex);
            }
            
            if (isRandom)
            {
                _words = _words.OrderBy(w => Guid.NewGuid()).ToList();
            }
            
            _currentPosition = 0;
        }
        
        public void Save(StreamWriter sw)
        {
            _save(sw, _words);
        }
        
        public void SaveAndOrderBy<TKey>(StreamWriter sw, Func<Word, TKey> keySelector)
        {
            var orderedWords = _words.OrderBy(keySelector).ToList();
            
            _save(sw, orderedWords);
        }
        
        private void _save(StreamWriter sw, List<Word> words)
        {
            foreach (var w in _words)
            {
                sw.WriteLine(string.Join("\t", w.Eng, w.Chn, w.Rating.ToString()));
            }
        }
        
        public void MoveNext()
        {
            if (_currentPosition < _words.Count - 1)
            {
                _currentPosition++;
            }
        }
        
        public void MovePrev()
        {
            if (_currentPosition > 0)
            {
                _currentPosition--;
            }
        }
        
        public void SetRating(WordRating rating)
        {
            switch (rating)
            {
                case WordRating.Good:
                    if (_words[_currentPosition].Rating > 0)
                    {
                        _words[_currentPosition].Rating--;
                    }
                    break;
                case WordRating.Soso:
                    _words[_currentPosition].Rating++;
                    break;
                case WordRating.Poor:
                    _words[_currentPosition].Rating += 2;
                    break;
            }
        }
        
        public bool EnterReview()
        {
            _backupWords = _words;
            
            _words = _getReviewList().ToList();
            
            if (_words.Count > 0)
            {
                _currentPosition = 0;
                return true;
            }
            
			_words = _backupWords;
            return false;
        }

        public void LeaveReview()
        {
            _words = _backupWords;
            _currentPosition = 0;
        }

        private IEnumerable<Word> _getReviewList()
        {
            foreach (var w in _backupWords)
            {
                if (w.Rating > 0)
                {
                    yield return w;
                }
            }
        }

		public void MoveTo(int position)
		{
			if (position < 0 || position >= _words.Count)
				throw new IndexOutOfRangeException ();

			_currentPosition = position;
		}

		public Word GetWordAt(int position)
		{
			return _words [position];
		}
    }
}

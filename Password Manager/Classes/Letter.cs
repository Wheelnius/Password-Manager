using System;
using System.Collections.Generic;
using System.Linq;

namespace Password_Manager.Classes
{
    public class Letter
    {
        private char let { get; set; }
        private int count { get; set; }
        private LinkedList<LetterCoord> loc = new LinkedList<LetterCoord>();
        private int usedLocCounter { get; set; } = -1;
        private int[] scrambleArray;
        private int hashCode;


        public Letter(char letter)
        {
            this.let = letter;
            this.count = 0;
        }

        public Letter(char letter, int hashCode)
        {
            this.let = letter;
            this.count = 0;
            this.hashCode = hashCode;
        }

        public int Count
        {
            get
            {
                return count;
            }
            set
            {
                this.count = value;
            }
        }

        public char Let
        {
            get
            {
                return let;
            }
            set
            {
                this.let = value;
            }
        }

        public int usedCount
        {
            get
            {
                return usedLocCounter;
            }
        }

        public void addToLetterCoords(int horizontal, int vertical)
        {
            this.loc.AddFirst(new LetterCoord(horizontal, vertical));
        }

        public void addToCount()
        {
            this.count++;
        }

        public LetterCoord useLocCoord()
        {
            usedLocCounter++;
            if (usedLocCounter >= count)
            {
                throw new IndexOutOfRangeException();
                usedLocCounter = 0; // Make this visible with a warning
               
            }
            return loc.ElementAt(scrambleArray[usedLocCounter]);
        }

        public void resetLocCount()
        {
            this.usedLocCounter = 0;
        }

        public LinkedList<LetterCoord> getLetterCoords()
        {
            return loc;
        }

        public void initScrambleArray()
        {
            Random rand = new Random(hashCode);
            scrambleArray = Enumerable.Range(0, count-1).ToArray();
            scrambleArray = scrambleArray.OrderBy(x => rand.Next()).ToArray();
        }
    }
}

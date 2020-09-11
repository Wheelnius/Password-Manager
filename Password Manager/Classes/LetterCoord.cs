using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Password_Manager.Classes
{
    public class LetterCoord
    {
        private int integerNumber { get; set; }
        private int letterID { get; set; }

        public LetterCoord(int integerNumber, int letterID)
        {
            this.integerNumber = integerNumber;
            this.letterID = letterID;
        }

        public int getIntegerNumber()
        {
            return integerNumber;
        }

        public int getLetterID()
        {
            return letterID;
        }

        public void setIntegerNumber(int value)
        {
            this.integerNumber = value;
        }

        public void setLetterID(int value)
        {
            this.letterID = value;
        }
    }
}

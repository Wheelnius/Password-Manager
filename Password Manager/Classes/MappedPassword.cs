using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Password_Manager.Classes
{

    class MappedPassword
    {
        private string name { get; set; }
        private string lineMap { get; set; }
        private LetterCoord[] mappings { get; set; }

        public MappedPassword(string name)
        {
            this.name = name;
            this.lineMap = "";
        }

        public void createLineMap(LetterCoord[] mappings)
        {
            this.mappings = mappings;

            for(int i=0; i<mappings.Length; i++)
            {
                try
                {
                    lineMap += mappings[i].getIntegerNumber() + " " + mappings[i].getLetterID() + " ";
                }
                catch (NullReferenceException)
                {
                    break;
                }
            }
        }

        public string getLineMap()
        {
            return lineMap;
        }

        public string getID1()
        {
            return name;
        }
    }
}

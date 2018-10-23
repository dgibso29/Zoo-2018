using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zoo
{
    public enum ErrorType { FailedClearanceCheck, CantAffordSingle, CantAffordMultiple, LandNotOwned}

    public static class ErrorHelper
    {
        public static Dictionary<ErrorType, string> ErrorDictionary;

        public delegate void ErrorOccurredEventHandler(object sender, ErrorOccurredEventArgs e);

        public static event ErrorOccurredEventHandler ErrorOccurred;

        public static void OnErrorOccurred(object sender, ErrorOccurredEventArgs e)
        {
            ErrorOccurred?.Invoke(sender, e);
        }


        public static void InitializeErrorDictionary()
        {
            if (ErrorDictionary == null)
            {
                ErrorDictionary = new Dictionary<ErrorType, string>();
                string[] errorTypes = new string[4];
                int index = 0;

                errorTypes[0] = "Cannot build here! Something is in the way!";
                errorTypes[index += 1] = "Cannot afford to build this!";
                errorTypes[index += 1] = "Cannot afford to build these!";
                errorTypes[index += 1] = "Cannot build here! Land is not owned by Zoo!";

                for (int i = 0; i < errorTypes.Count(); i++)
                {
                    ErrorDictionary.Add((ErrorType)i, errorTypes[i]);
                }
            }
        }
    }
}

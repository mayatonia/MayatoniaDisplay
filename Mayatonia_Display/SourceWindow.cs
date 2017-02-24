using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Markup;

namespace MayatoniaDisplay
{
    public class SourceWindow : Window
    {
        DisplaySource originalSource;
        DisplaySource savedSource;
        Settings originalSettings;
        Settings savedSettings;

        public DisplaySource OriginalSource
        {
            get
            {
                return originalSource;
            }

            set
            {
                originalSource = value;
            }
        }

        public DisplaySource SavedSource
        {
            get
            {
                return savedSource;
            }

            set
            {
                savedSource = value;
            }
        }

        public bool IsInDesignMode
        {
            get
            {
                return System.ComponentModel.
                        DesignerProperties.GetIsInDesignMode(this);
            }
        }

        public Settings OriginalSettings
        {
            get
            {
                return originalSettings;
            }

            set
            {
                originalSettings = value;
            }
        }

        public Settings SavedSettings
        {
            get
            {
                return savedSettings;
            }

            set
            {
                savedSettings = value;
            }
        }

        public void OpenResourceDictionary(string fileName)
            {
                ResourceDictionary dic = null;

                if (File.Exists(fileName))
                {
                    using (FileStream fs = new FileStream(fileName, FileMode.Open))
                        dic = (ResourceDictionary)XamlReader.Load(fs);

                    this.Resources.MergedDictionaries.Add(dic);
                }
                else
                    throw new FileNotFoundException(
                      "Can't open resource file: " + fileName +
                      " in the method OpenResourceDictionary().");
            }

    }

}

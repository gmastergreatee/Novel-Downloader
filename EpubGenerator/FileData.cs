using System;
using System.Text;

namespace EpubGenerator
{
    public class FileData
    {
        bool isCoverImage = false;
        EnumFileType fileType = EnumFileType.XHTML;
        bool isScripted = false;

        public string FileName { get; set; } = "";
        internal byte[] FileBytes { get; set; } = null;

        /// <summary>
        /// Default XHTML, changing this resets CoverImage flag
        /// </summary>
        public EnumFileType FileType
        {
            get
            {
                return fileType;
            }
            set
            {
                fileType = value;
                isCoverImage = false;
            }
        }

        /// <summary>
        /// Default false
        /// </summary>
        public bool IsCoverImage
        {
            get
            {
                return isCoverImage;
            }
            set
            {
                if (value && (fileType == EnumFileType.JPEG || fileType == EnumFileType.PNG))
                    isCoverImage = true;
                else if (value)
                    throw new Exception("Only JPEG & PNG images can be set as CoverImage");
                else
                    isCoverImage = false;
            }
        }
        
        /// <summary>
        /// Default false
        /// </summary>
        public bool IsScripted
        {
            get
            {
                return isScripted;
            }
            set
            {
                if (value && fileType == EnumFileType.XHTML)
                    isScripted = true;
                else if (value)
                    throw new Exception("Only XHTML filetype can be set as Scripted");
                else
                    isScripted = false;
            }
        }

        public void PutBytes(byte[] bytes)
        {
            FileBytes = bytes;
        }

        public void PutString(string text)
        {
            FileBytes = Encoding.UTF8.GetBytes(text);
        }
    }
}

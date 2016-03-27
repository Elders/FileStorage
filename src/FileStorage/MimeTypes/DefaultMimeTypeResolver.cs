using System;
using System.Collections.Generic;
using System.Linq;

namespace FileStorage.MimeTypes
{
    public class DefaultMimeTypeResolver : IMimeTypeResolver
    {
        string defaultMimeType = "application/octet-stream";

        IList<MimeTypeMapper> mappings = new List<MimeTypeMapper>
            {
                new MimeTypeMapper( "BMP", "image/bmp", new MimeTypePattern(new byte[] { 66, 77 }) ),
                new MimeTypeMapper( "DOC", "application/msword", new MimeTypePattern(new byte[] { 208, 207, 17, 224, 161, 177, 26, 225 } )),
                new MimeTypeMapper( "EXE_DLL", "application/x-msdownload",new MimeTypePattern( new byte[] { 77, 90 } )),
                new MimeTypeMapper( "GIF", "image/gif", new MimeTypePattern( new byte[] { 71, 73, 70, 56 } )),
                new MimeTypeMapper( "ICO", "image/x-icon", new MimeTypePattern( new byte[] { 0, 0, 1, 0 } )),
                new MimeTypeMapper( "JPG", "image/jpeg", new MimeTypePattern( new byte[] { 255, 216, 255 } )),
                new MimeTypeMapper( "MP3", "audio/mpeg", new MimeTypePattern(new byte[] { 255, 251, 48 } )),
                new MimeTypeMapper( "OGG", "video/ogg", new MimeTypePattern(new byte[] { 79, 103, 103, 83, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0 } )),
                new MimeTypeMapper( "PDF", "application/pdf", new MimeTypePattern( new byte[] { 37, 80, 68, 70, 45, 49, 46 } )),
                new MimeTypeMapper( "PNG", "image/png", new MimeTypePattern( new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82 } )),
                new MimeTypeMapper( "RAR", "application/x-rar-compressed",new MimeTypePattern( new byte[] { 82, 97, 114, 33, 26, 7, 0 } )),
                new MimeTypeMapper( "SWF", "application/x-shockwave-flash",new MimeTypePattern( new byte[] { 70, 87, 83 } )),
                new MimeTypeMapper( "TIFF", "image/tiff", new MimeTypePattern( new byte[] { 73, 73, 42, 0 } )),
                new MimeTypeMapper( "TORRENT", "application/x-bittorrent", new MimeTypePattern( new byte[] { 100, 56, 58, 97, 110, 110, 111, 117, 110, 99, 101 })),
                new MimeTypeMapper( "TTF", "application/x-font-ttf", new MimeTypePattern(new byte[] { 0, 1, 0, 0, 0 } )),
                new MimeTypeMapper( "WAV_AVI", "video/x-msvideo", new MimeTypePattern( new byte[] { 82, 73, 70, 70 } )),
                new MimeTypeMapper( "WMV_WMA", "audio/x-ms-wma", new MimeTypePattern(new byte[] { 48, 38, 178, 117, 142, 102, 207, 17, 166, 217, 0, 170, 0, 98, 206, 108 } )),
                new MimeTypeMapper( "ZIP_DOCX", "application/x-zip-compressed", new MimeTypePattern(new byte[] { 80, 75, 3, 4 } )),
                new MimeTypeMapper( "SEVEN_ZIP", "application/x-7z-compressed", new MimeTypePattern( new byte[] { 55, 122 } )),

                // http://www.garykessler.net/library/file_sigs.html
                new MimeTypeMapper( "AVI", "video/x-msvideo", new MimeTypePattern("52 49 46 46" )),
                new MimeTypeMapper( "AVI", "video/x-msvideo", new MimeTypePattern("41 56 49 20 4C 49 53 54" )),
                new MimeTypeMapper( "MP4", "video/mp4", new MimeTypePattern( "66 74 79 70 33 67 70 35", 4) ),
                new MimeTypeMapper( "MP4", "video/mp4", new MimeTypePattern( "66 74 79 70 4D 53 4E 56", 4 )),
                new MimeTypeMapper( "MP4", "video/mp4", new MimeTypePattern("00 00 00 14 66 74 79 70 69 73 6F 6D") ),
                new MimeTypeMapper( "MP4", "video/mp4", new MimeTypePattern("00 00 00 18 66 74 79 70 33 67 70 35") ),
                new MimeTypeMapper( "MP4", "video/mp4", new MimeTypePattern("00 00 00 1C 66 74 79 70 4D 53 4E 56 01 29 00 46 4D 53 4E 56 6D 70 34 32" )),
                new MimeTypeMapper( "MP4", "video/mp4", new MimeTypePattern("66 74 79 70 69 73 6F 6D", 4)),
                new MimeTypeMapper( "ASF_WMV", "video/x-ms-wmv", new MimeTypePattern("30 26 B2 75 8E 66 CF 11 A6 D9 00 AA 00 62 CE 6C" )),
                new MimeTypeMapper( "MPG", "video/mpeg", new MimeTypePattern("00 00 01 B3" )),
                new MimeTypeMapper( "MPG", "video/mpeg", new MimeTypePattern("00 00 01 BA" )),
                new MimeTypeMapper( "FLV", "video/x-flv", new MimeTypePattern("46 4C 56 01" )),
                new MimeTypeMapper( "JPG", "image/jpeg", new MimeTypePattern("FF D8 FF E0" )),
                new MimeTypeMapper( "JPG", "image/jpeg", new MimeTypePattern("FF D8 FF E1" )),
                new MimeTypeMapper( "JPG", "image/jpeg", new MimeTypePattern("FF D8 FF E8" )),
                new MimeTypeMapper( "JPG", "image/bmp", new MimeTypePattern("42 4D" )),
                new MimeTypeMapper( "GIF", "image/gif", new MimeTypePattern("47 49 46 38 37 61" )),
                new MimeTypeMapper( "GIF", "image/gif", new MimeTypePattern("47 49 46 38 39 61" ))
            };

        public string GetMimeType(byte[] data)
        {
            foreach (var mapping in mappings)
            {
                if (mapping.Pattern.IsMatch(data))
                    return mapping.Mime;
            }

            return defaultMimeType;
        }

        public IReadOnlyCollection<string> SupportedTypes
        {
            get { return mappings.Select(x => x.Name).Distinct((StringComparer.InvariantCultureIgnoreCase)).ToList().AsReadOnly(); }
        }
    }
}
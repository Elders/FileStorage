using System;
using System.Collections.Generic;
using System.Linq;

namespace FileStorage.MimeTypes
{
    public class DefaultMimeTypeResolver : IMimeTypeResolver
    {
        readonly IList<MimeTypeMapper> mappings = new List<MimeTypeMapper>
            {
                new MimeTypeMapper( "BMP", ".bmp", "image/bmp", new MimeTypePattern(new byte[] { 66, 77 }) ),
                new MimeTypeMapper( "DOC", ".doc", "application/msword", new MimeTypePattern(new byte[] { 208, 207, 17, 224, 161, 177, 26, 225 } )),
                new MimeTypeMapper( "EXE_DLL", ".exe", "application/x-msdownload",new MimeTypePattern( new byte[] { 77, 90 } )),
                new MimeTypeMapper( "GIF", ".gif", "image/gif", new MimeTypePattern( new byte[] { 71, 73, 70, 56 } )),
                new MimeTypeMapper( "ICO", ".ico", "image/x-icon", new MimeTypePattern( new byte[] { 0, 0, 1, 0 } )),
                new MimeTypeMapper( "JPG", ".jpg", "image/jpeg", new MimeTypePattern( new byte[] { 255, 216, 255 } )),
                new MimeTypeMapper( "MP3", ".mpga", "audio/mpeg", new MimeTypePattern(new byte[] { 255, 251, 48 } )),
                new MimeTypeMapper( "OGG", ".ogv", "video/ogg", new MimeTypePattern(new byte[] { 79, 103, 103, 83, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0 } )),
                new MimeTypeMapper( "PDF", ".pdf", "application/pdf", new MimeTypePattern( new byte[] { 37, 80, 68, 70, 45, 49, 46 } )),
                new MimeTypeMapper( "PNG", ".png", "image/png", new MimeTypePattern( new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82 } )),
                new MimeTypeMapper( "RAR", ".rar", "application/x-rar-compressed",new MimeTypePattern( new byte[] { 82, 97, 114, 33, 26, 7, 0 } )),
                new MimeTypeMapper( "SWF", ".swf", "application/x-shockwave-flash",new MimeTypePattern( new byte[] { 70, 87, 83 } )),
                new MimeTypeMapper( "TIFF", ".tiff", "image/tiff", new MimeTypePattern( new byte[] { 73, 73, 42, 0 } )),
                new MimeTypeMapper( "TORRENT", ".torrent", "application/x-bittorrent", new MimeTypePattern( new byte[] { 100, 56, 58, 97, 110, 110, 111, 117, 110, 99, 101 })),
                new MimeTypeMapper( "TTF", ".ttf", "application/x-font-ttf", new MimeTypePattern(new byte[] { 0, 1, 0, 0, 0 } )),
                new MimeTypeMapper( "WAV_AVI", ".avi", "video/x-msvideo", new MimeTypePattern( new byte[] { 82, 73, 70, 70 } )),
                new MimeTypeMapper( "WMV_WMA", ".wma", "audio/x-ms-wma", new MimeTypePattern(new byte[] { 48, 38, 178, 117, 142, 102, 207, 17, 166, 217, 0, 170, 0, 98, 206, 108 } )),
                new MimeTypeMapper( "ZIP_DOCX", ".zip", "application/x-zip-compressed", new MimeTypePattern(new byte[] { 80, 75, 3, 4 } )),
                new MimeTypeMapper( "SEVEN_ZIP", ".7z", "application/x-7z-compressed", new MimeTypePattern( new byte[] { 55, 122 } )),

                // http://www.garykessler.net/library/file_sigs.html
                // http://string-functions.com/hex-string.aspx
                new MimeTypeMapper( "AVI", ".avi", "video/x-msvideo", new MimeTypePattern("52 49 46 46" )),
                new MimeTypeMapper( "AVI", ".avi",  "video/x-msvideo", new MimeTypePattern("41 56 49 20 4C 49 53 54" )),
                new MimeTypeMapper( "MP4", ".mp4", "video/mp4", new MimeTypePattern( "66 74 79 70 33 67 70 35", 4) ),
                new MimeTypeMapper( "MP4", ".mp4", "video/mp4", new MimeTypePattern( "66 74 79 70 4D 53 4E 56", 4 )),
                new MimeTypeMapper( "MP4", ".mp4", "video/mp4", new MimeTypePattern("00 00 00 14 66 74 79 70 69 73 6F 6D") ),
                new MimeTypeMapper( "MP4", ".mp4", "video/mp4", new MimeTypePattern("00 00 00 18 66 74 79 70 33 67 70 35") ),
                new MimeTypeMapper( "MP4", ".mp4", "video/mp4", new MimeTypePattern("00 00 00 1C 66 74 79 70 4D 53 4E 56 01 29 00 46 4D 53 4E 56 6D 70 34 32" )),
                new MimeTypeMapper( "MP4", ".mp4", "video/mp4", new MimeTypePattern("00 00 00 1C 66 74 79 70 6D 70 34 32 00 00 00 01 6D 70 34 31 6D 70 34 32 69 73 6F 6D 00 00")),  // ftypmp?42mp41?mp42isom | https://en.wikipedia.org/wiki/ISO_base_media_file_format
                new MimeTypeMapper( "MP4", ".mp4", "video/mp4", new MimeTypePattern("66 74 79 70 69 73 6F 6D", 4)),
                new MimeTypeMapper( "MP4", ".mp4", "video/mp4", new MimeTypePattern("66 74 79 70 6D 70 34 32", 4)), // ftypmp42
                new MimeTypeMapper( "MP4", ".mp4", "video/mp4", new MimeTypePattern("00 00 00 20 66 74 79 70 69 73 6F 36 00 00 00 01 6D 70 34 32 69 73 6F 36 61 76 63 31 69 73 6F 6D")), // this is added because of https://github.com/titansgroup/k4l-video-trimmer | ftypiso6 mp42iso6avc1isom
                new MimeTypeMapper( "ASF_WMV", ".wmv", "video/x-ms-wmv", new MimeTypePattern("30 26 B2 75 8E 66 CF 11 A6 D9 00 AA 00 62 CE 6C" )),
                new MimeTypeMapper( "MPG", ".mpeg", "video/mpeg", new MimeTypePattern("00 00 01 B3" )),
                new MimeTypeMapper( "MPG", ".mpeg", "video/mpeg", new MimeTypePattern("00 00 01 BA" )),
                new MimeTypeMapper( "FLV", ".flv", "video/x-flv", new MimeTypePattern("46 4C 56 01" )),
                new MimeTypeMapper( "JPG", ".jpg", "image/jpeg", new MimeTypePattern("FF D8 FF E0" )),
                new MimeTypeMapper( "JPG", ".jpg", "image/jpeg", new MimeTypePattern("FF D8 FF E1" )),
                new MimeTypeMapper( "JPG", ".jpg", "image/jpeg", new MimeTypePattern("FF D8 FF E8" )),
                new MimeTypeMapper( "JPG", ".bmp", "image/bmp", new MimeTypePattern("42 4D" )),
                new MimeTypeMapper( "GIF", ".gif", "image/gif", new MimeTypePattern("47 49 46 38 37 61" )),
                new MimeTypeMapper( "GIF", ".gif", "image/gif", new MimeTypePattern("47 49 46 38 39 61" ))
            };

        public string GetMimeType(byte[] data)
        {
            foreach (var mapping in mappings)
            {
                if (mapping.Pattern.IsMatch(data))
                    return mapping.Mime;
            }

            return DefaultMimeType;
        }

        public string GetExtension(byte[] data)
        {
            foreach (var mapping in mappings)
            {
                if (mapping.Pattern.IsMatch(data))
                    return mapping.Extension;
            }

            return string.Empty;
        }

        public IReadOnlyCollection<string> SupportedTypes
        {
            get { return mappings.Select(x => x.Name).Distinct((StringComparer.InvariantCultureIgnoreCase)).ToList().AsReadOnly(); }
        }

        public string DefaultMimeType
        {
            get
            {
                return "application/octet-stream";
            }
        }
    }
}

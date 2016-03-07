using ImageResizer;
using System.Collections.Generic;
using ImageResizer.Resizing;
using ImageResizer.Encoding;
using ImageResizer.Plugins;

namespace FileStorage.FileFormats
{
    public class CustomImageBuilder : ImageBuilder
    {
        public CustomImageBuilder() { }

        public CustomImageBuilder(IEnumerable<BuilderExtension> extensions, IEncoderProvider encoderProvider, IVirtualImageProvider virtualFileProvider, ISettingsModifier settingsModifier)
            : base(extensions, encoderProvider, virtualFileProvider, settingsModifier)
        { }

        public override ImageBuilder Create(IEnumerable<BuilderExtension> extensions, IEncoderProvider writer, IVirtualImageProvider virtualFileProvider, ISettingsModifier settingsModifier)
        {
            return new CustomImageBuilder(extensions, writer, virtualFileProvider, settingsModifier);
        }

        protected override RequestedAction OnProcess(ImageState s)
        {
            if (s.originalSize.Width < s.originalSize.Height && s.settings.Width > s.settings.Height)
            {
                s.settings.Width = s.settings.Width + s.settings.Height;
                s.settings.Height = s.settings.Width - s.settings.Height;
                s.settings.Width = s.settings.Width - s.settings.Height;
            }
            return base.OnProcess(s);
        }
    }
}

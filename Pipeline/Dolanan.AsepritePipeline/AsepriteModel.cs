﻿using System;

namespace Dolanan.AsepritePipeline
{

    public partial class AsepriteModel
    {
        public FrameElement[] Frames { get; set; }
        public Meta Meta { get; set; }
    }

    public partial class FrameElement
    {
        public string Filename { get; set; }
        public SpriteSourceSizeClass Frame { get; set; }
        public bool Rotated { get; set; }
        public bool Trimmed { get; set; }
        public SpriteSourceSizeClass SpriteSourceSize { get; set; }
        public Size SourceSize { get; set; }
        public long Duration { get; set; }
    }

    public partial class SpriteSourceSizeClass
    {
        public long X { get; set; }
        public long Y { get; set; }
        public long W { get; set; }
        public long H { get; set; }
    }

    public partial class Size
    {
        public long W { get; set; }
        public long H { get; set; }
    }

    public partial class Meta
    {
        public Uri App { get; set; }
        public string Version { get; set; }
        public string Image { get; set; }
        public string Format { get; set; }
        public Size Size { get; set; }
        public long Scale { get; set; }
        public FrameTag[] FrameTags { get; set; }
        public Layer[] Layers { get; set; }
        public Slice[] Slices { get; set; }
    }

    public partial class FrameTag
    {
        public string Name { get; set; }
        public long From { get; set; }
        public long To { get; set; }
        public string Direction { get; set; }
    }

    public partial class Layer
    {
        public string Name { get; set; }
        public long Opacity { get; set; }
        public string BlendMode { get; set; }
    }

    public partial class Slice
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public Key[] Keys { get; set; }
    }

    public partial class Key
    {
        public long Frame { get; set; }
        public SpriteSourceSizeClass Bounds { get; set; }
        public SpriteSourceSizeClass Center { get; set; }
        public Pivot Pivot { get; set; }
    }

    public partial class Pivot
    {
        public long X { get; set; }
        public long Y { get; set; }
    }
}

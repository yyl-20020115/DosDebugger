﻿using System;

namespace Disassembler;

public class Executable : Assembly
{
    public readonly ExecutableImage Image;
    public readonly Address EntryPoint;

    public Executable(string fileName)
    {
        var file = new MZFile(fileName);

        this.Image = new ExecutableImage(file);
        this.EntryPoint = new (Image.MapFrameToSegment(file.EntryPoint.Segment), file.EntryPoint.Offset);
    }

    public override BinaryBaseImage GetImage() => Image;

    public static Address PointerToAddress(FarPointer pointer) => new(pointer.Segment, pointer.Offset);

    /// <summary>
    /// Gets the entry point address of the executable.
    /// </summary>

}

#if true
public class LoadModule(ImageChunk image) : Module
{
    public readonly ImageChunk image = image ?? throw new ArgumentNullException("image");

    /// <summary>
    /// Gets the binary image of the load module.
    /// </summary>
    public ImageChunk Image => image;

    /// <summary>
    /// Gets or sets the initial value of SS register. This value must be
    /// relocated when the image is loaded.
    /// </summary>
    public UInt16 InitialSS { get; set; }

    /// <summary>
    /// Gets or sets the initial value of SP register.
    /// </summary>
    public UInt16 InitialSP { get; set; }

    /// <summary>
    /// Gets or sets the initial value of CS register. This value must be
    /// relocated when the image is loaded.
    /// </summary>
    public UInt16 InitialCS { get; set; }

    /// <summary>
    /// Gets or sets the initial value of IP register.
    /// </summary>
    public UInt16 InitialIP { get; set; }
}
#endif

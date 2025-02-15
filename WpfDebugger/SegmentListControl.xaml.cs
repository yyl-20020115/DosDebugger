﻿using System;
using System.Windows.Controls;
using Disassembler;

namespace WpfDebugger;

/// <summary>
/// Interaction logic for SegmentListControl.xaml
/// </summary>
public partial class SegmentListControl : UserControl
{
    public SegmentListControl()
    {
        InitializeComponent();
    }

    private Assembly program;

    public Assembly Program
    {
        get => program;
        set
        {
            program = value;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        lvSegments.ItemsSource = null;
        if (program == null)
            return;

#if false
        var items = (from segment in program.Segments
                     select new SegmentViewItem(segment)
                     ).ToArray();
        lvSegments.ItemsSource = items;
#endif
    }

#if false
    private void lvSegments_DoubleClick(object sender, EventArgs e)
    {
        if (lvSegments.SelectedIndices.Count == 0)
            return;

        Segment segment = (Segment)lvSegments.SelectedItems[0].Tag;
        document.Navigator.SetLocation(
            segment.StartAddress.ToFarPointer(segment.SegmentAddress), this);
    }
#endif
}

class SegmentViewItem(Segment segment)
{
    public Segment Segment { get; private set; } = segment ?? throw new ArgumentNullException(nameof(segment));

    public string Start => "NA";

    public string End => "NA";

#if false
    private static string FormatAddress(LinearPointer address, UInt16 segment)
    {
        return string.Format(
            "{0} ({1:X5})", new Pointer(segment, address), address);
    }
#endif
}

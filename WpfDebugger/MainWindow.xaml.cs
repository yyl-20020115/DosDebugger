﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Disassembler;
using Xceed.Wpf.AvalonDock.Layout;

namespace WpfDebugger;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Try load the last layout.
        try
        {
            LoadDockingLayout();
        }
        catch (Exception)
        {
        }
    }

    Assembly program;

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        //var fileName = @"..\Data\SPTABLET.COM";
        //DoOpenFile(fileName);
    }

    private void Window_Unloaded(object sender, RoutedEventArgs e)
    {
        try
        {
            SaveDockingLayout();
        }
        catch (Exception)
        {
        }
    }

    #region Docking Layout Save/Load

    private void SaveDockingLayout()
    {
        var serializer = new Xceed.Wpf.AvalonDock.Layout.Serialization.XmlLayoutSerializer(
            dockingManager);
        using var stream = System.IO.File.Create("AvalonLayoutConfig.xml");
        serializer.Serialize(stream);
    }

    private void LoadDockingLayout()
    {
        var serializer = new Xceed.Wpf.AvalonDock.Layout.Serialization.XmlLayoutSerializer(
            dockingManager);
        //serializer.LayoutSerializationCallback += serializer_LayoutSerializationCallback;
        using var stream = File.OpenRead("AvalonLayoutConfig.xml");
        serializer.Deserialize(stream);
    }

    private void MnuFileSaveLayout_Click(object sender, RoutedEventArgs e)
    {
        SaveDockingLayout();
        MessageBox.Show("Layout saved.");
    }

    private void MnuFileLoadLayout_Click(object sender, RoutedEventArgs e)
    {
        LoadDockingLayout();
    }

    #endregion

    private void DoOpenFile(string fileName)
    {
        if (fileName.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
        {
            DoOpenExeFile(fileName);
        }
        else if (fileName.EndsWith(".lib", StringComparison.InvariantCultureIgnoreCase))
        {
            DoOpenLibFile(fileName);
        }
        else if (fileName.EndsWith(".obj", StringComparison.InvariantCultureIgnoreCase))
        {
            DoOpenObjFile(fileName);
        }
        else
        {
            MessageBox.Show("File type is not supported.");
        }
    }

    private void DoOpenExeFile(string fileName)
    {
        //MZFile mzFile = new MZFile(fileName);
        //mzFile.Relocate(0x1000); // TODO: currently we don't support loadin
        // at segment 0. We should fix this later.
        var executable = new Executable(fileName);
        var dasm = new ExecutableDisassembler(executable);
        dasm.Analyze();

        this.program = executable;
        //this.disassemblyList.Image = image;
        this.procedureList.Program=program;
        this.errorList.Program = program;
        this.segmentList.Program = program;
        // this.propertiesWindow.Image = image;
    }

    private void DoOpenLibFile(string fileName)
    {
        var library = OmfLoader.LoadLibrary(fileName);
        library.ResolveAllSymbols();

        var dasm = new LibraryDisassembler(library);
        dasm.Analyze();

        this.program = library;
        this.procedureList.Program = program;
        this.errorList.Program = program;
        this.segmentList.Program = program;

        // Display all unresolved symbols.
        foreach (var key in library.GetUnresolvedSymbols())
        {
            System.Diagnostics.Debug.WriteLine(string.Format(
                "Symbol {0} is unresolved.", key));
        }

        this.libraryBrowser.Library = library;

#if false
        string symbolToFind = "FISRQQ";
        foreach (var mod in library.Symbols[symbolToFind])
        {
            System.Diagnostics.Debug.WriteLine(string.Format(
                "Symbol {0} is defined in module {1}",
                symbolToFind, mod.Name));
        }
        //library.DuplicateSymbols

        ObjectModule module = library.FindModule("_ctype");
        DefinedSymbol symbol = module.DefinedNames.Find(x => x.Name == "_isupper");
        Address entryPoint = new Address(
            symbol.BaseSegment.Id, (int)symbol.Offset);

        Disassembler16New dasm = new Disassembler16New(library);
        dasm.Analyze(entryPoint);

        this.disassemblyList.SetView(library, symbol.BaseSegment);
#endif
    }

    private void DoOpenObjFile(string fileName)
    {
        var library = OmfLoader.LoadObject(fileName);
        library.ResolveAllSymbols();

        var dasm = new LibraryDisassembler(library);
        dasm.Analyze();

        this.program = library;
        this.procedureList.Program = program;
        this.errorList.Program = program;
        this.segmentList.Program = program;
        this.libraryBrowser.Library = library;
    }

    private void MnuHelpTest_Click(object sender, RoutedEventArgs e)
    {
        //string fileName = @"..\..\..\..\Test\SLIBC7.LIB";
        //DoOpenLibFile(fileName);
    }

    private void MnuFileExit_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    #region Tool Window Activation

    private void MnuViewSegments_Click(object sender, RoutedEventArgs e)
    {
        ActivateToolWindow(segmentList);
    }

    private void MnuViewErrors_Click(object sender, RoutedEventArgs e)
    {
        ActivateToolWindow(errorList);
    }

    private void MnuViewProcedures_Click(object sender, RoutedEventArgs e)
    {
        ActivateToolWindow(procedureList);
    }

    private void MnuViewProperties_Click(object sender, RoutedEventArgs e)
    {
        ActivateToolWindow(propertiesWindow);
    }

    private void MnuViewLibraryBrowser_Click(object sender, RoutedEventArgs e)
    {
        ActivateToolWindow(libraryBrowser);
    }

    /// <summary>
    /// Activates the dockable pane that contains the given control.
    /// The search is performed by matching the pane's ContentId to the
    /// controls's Name. If no dockable pane contains the control, one is
    /// created at the bottom side of the docking root; in this case, the
    /// control's ToolTip (if it is a non-null string) is used as the
    /// pane's Title.
    /// </summary>
    /// <param name="control">The control to activate.</param>
    /// <remarks>
    /// This code is partly adapted from AvalonDock samples. It's not
    /// clear how it's done, but normally it works.
    /// </remarks>
    private void ActivateToolWindow(Control control)
    {
        if (control == null)
            throw new ArgumentNullException(nameof(control));

        var contentId = control.Name;

        var pane = dockingManager.Layout.Descendents().OfType<
            LayoutAnchorable>().SingleOrDefault(a => a.ContentId == contentId);

        if (pane == null)
        {
            // The pane is not created. This can happen for example when
            // we load from an old layout configuration file, and the
            // pane is not defined in that file. In this case, we add the
            // control to a default location.
            var anchorSide = dockingManager.BottomSidePanel.Model as LayoutAnchorSide;
            LayoutAnchorGroup anchorGroup;
            if (anchorSide.ChildrenCount == 0)
            {
                anchorGroup = new LayoutAnchorGroup();
                anchorSide.Children.Add(anchorGroup);
            }
            else
            {
                anchorGroup = anchorSide.Children[0];
            }

            pane = new LayoutAnchorable
            {
                ContentId = contentId,
                Content = control
            };
            if (control.ToolTip is string s)
            {
                pane.Title = s;
            }
            anchorGroup.Children.Add(pane);
        }

        if (pane.IsHidden)
        {
            pane.Show();
        }
        else if (pane.IsVisible)
        {
            pane.IsActive = true;
        }
        else
        {
            pane.AddToLayout(dockingManager,
                AnchorableShowStrategy.Bottom |
                AnchorableShowStrategy.Most);
        }

        //control.Focus
        //if (!control.Focus())
        //    throw new InvalidOperationException();
        //Keyboard.Focus(control);
    }

    #endregion

    #region Select Theme

    private void MnuViewThemeItem_Click(object sender, RoutedEventArgs e)
    {
        mnuViewThemeDefault.IsChecked = false;
        mnuViewThemeAero.IsChecked = false;
        mnuViewThemeExpressionDark.IsChecked = false;
        mnuViewThemeExpressionLight.IsChecked = false;
        mnuViewThemeMetro.IsChecked = false;
        mnuViewThemeVS2010.IsChecked = false;

        //if (sender == mnuViewThemeVS2010)
        //    dockingManager.Theme = new Xceed.Wpf.AvalonDock.Themes.VS2010Theme();
        //else if (sender == mnuViewThemeExpressionDark)
        //    dockingManager.Theme = new Xceed.Wpf.AvalonDock.Themes.ExpressionDarkTheme();
        //else if (sender == mnuViewThemeExpressionLight)
        //    dockingManager.Theme = new Xceed.Wpf.AvalonDock.Themes.ExpressionLightTheme();
        //else if (sender == mnuViewThemeAero)
        //    dockingManager.Theme = new Xceed.Wpf.AvalonDock.Themes.AeroTheme();
        //else if (sender == mnuViewThemeMetro)
        //    dockingManager.Theme = new Xceed.Wpf.AvalonDock.Themes.MetroTheme();
        //else
        //    dockingManager.Theme = null;

        ((MenuItem)sender).IsChecked = true;
    }
    
    #endregion

    private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        //MessageBox.Show(string.Format(
        //    "Navigating to {0} in {1}", e.Uri, e.Target));
        //Pointer address = Pointer.Parse(e.Uri.Fragment.Substring(1)); // skip #
        //disassemblyList.GoToAddress(address);

        var uri = e.Uri as AssemblyUri;
        if (uri == null)
            return;

#if false
        if (uri.Referent is Segment)
        {
            this.disassemblyList.SetView(program, uri.Referent as Segment, uri.Offset);
        }
        else
        {
            MessageBox.Show("Not supported");
        }
#else
        this.disassemblyList.SetView(program, uri.Address);
#endif
    }

    private void FileOpenCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void FileOpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var dlg = new Microsoft.Win32.OpenFileDialog();
        dlg.Filter =
            "All supported files|*.exe;*.lib;*.obj" +
            "|Executable files|*.exe" +
            "|Library files|*.lib" +
            "|Object files|*.obj";

        dlg.Title = "Select File To Analyze";

        if (dlg.ShowDialog(this) == true)
        {
            DoOpenFile(dlg.FileName);
        }
    }

    private void MnuHelpAbout_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(this, "DOS Disassembler\r\nCopyright fanci 2012-2013\r\n",
                        "About", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void LibraryBrowser_RequestProperty(object sender, RequestPropertyEventArgs e)
    {
        propertiesWindow.SelectedObject = e.SelectedObject;
    }

    private void MnuToolsExportChecksum_Click(object sender, RoutedEventArgs e)
    {
        if (program == null)
            return;

        var image = program.GetImage();
        using var writer = new StreamWriter(@"E:\TestDDD-Procedures.txt");
        foreach (var procedure in image.Procedures)
        {
            var checksum = CodeChecksum.Compute(procedure, image);
            writer.WriteLine("{0} {1} {2} {3}",
                             image.FormatAddress(procedure.EntryPoint),
                             procedure.Name,
                             BytesToString(checksum.OpcodeChecksum).ToLowerInvariant(),
                             procedure.Size);
        }
    }

    private static string BytesToString(byte[] bytes)
    {
        //var soapBinary = new System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary(bytes);
        var builder = new StringBuilder();
        foreach(var b in bytes)
        {
            builder.AppendFormat("{0:X2", b);
        }
        return builder.ToString();
    }
}

using pkuManager.Alerts;

namespace pkuManager.GUI;

/// <summary>
/// A GUI container for note type Alerts. To be used in conjunction with the porter windows.
/// </summary>
public class NoteBox : AlertBox
{
    protected override int boxWidth => 300;
    protected override int boxMaxHeight => 200;
    protected override int textBoxMaxWidth => 270;

    public NoteBox(Alert alert) : base(alert) { }
}
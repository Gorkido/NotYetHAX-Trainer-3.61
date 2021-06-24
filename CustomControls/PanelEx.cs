using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

[DesignerCategory("code")]
public class PanelEx : Panel
{
    private Color m_BorderColorSel = Color.Transparent;
    private Color m_BorderColor = Color.Transparent;
    private bool m_Selectable = false;
    private bool m_Selected = false;
    private int m_BorderSize = 1;

    public PanelEx() { }

    public Color BorderColor
    {
        get => m_BorderColor;
        set
        {
            if (value == m_BorderColor) return;
            m_BorderColor = value;
            Invalidate();
        }
    }

    public int BorderSize
    {
        get => m_BorderSize;
        set
        {
            if (value == m_BorderSize) return;
            m_BorderSize = value;
            Invalidate();
        }
    }

    public bool Selectable
    {
        get => m_Selectable;
        set
        {
            if (value == m_Selectable) return;
            m_Selectable = value;
            SetStyle(ControlStyles.Selectable | ControlStyles.UserMouse | ControlStyles.StandardClick, value);
            this.UpdateStyles();
            this.Invalidate();
        }
    }

    public Color BorderColorSelected
    {
        get => m_BorderColorSel;
        set
        {
            m_BorderColorSel = value;
            if (!Selectable || value == m_BorderColorSel) return;
            Invalidate();
        }
    }


    protected override void OnPaint(PaintEventArgs e)
    {
        Color penColor = m_Selectable && m_Selected ? m_BorderColorSel : m_BorderColor;
        int rectOffset = BorderSize / 2;
        using (Pen pen = new Pen(penColor, BorderSize))
        {
            var rect = new Rectangle(rectOffset, rectOffset, ClientSize.Width - BorderSize, ClientSize.Height - BorderSize);
            e.Graphics.DrawRectangle(pen, rect);
        }
        base.OnPaint(e);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        this.OnEnter(e);
    }
    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        this.OnLeave(e);
    }

    protected override void OnEnter(EventArgs e)
    {
        base.OnEnter(e);
        m_Selected = true;
        Invalidate();
    }
    protected override void OnLeave(EventArgs e)
    {
        base.OnLeave(e);
        m_Selected = false;
        Invalidate();
    }
}
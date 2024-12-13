<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class IO_frm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.hide_btn = New System.Windows.Forms.Button()
        'Me.DigitalIOCtrl1 = New dio_ctrl.DigitalIOCtrl()
        Me.SuspendLayout()
        '
        'hide_btn
        '
        Me.hide_btn.Location = New System.Drawing.Point(105, 358)
        Me.hide_btn.Name = "hide_btn"
        Me.hide_btn.Size = New System.Drawing.Size(57, 23)
        Me.hide_btn.TabIndex = 6
        Me.hide_btn.Text = "Hide"
        Me.hide_btn.UseVisualStyleBackColor = True
        '
        'DigitalIOCtrl1
        '
        'Me.DigitalIOCtrl1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        'Me.DigitalIOCtrl1.Location = New System.Drawing.Point(9, 9)
        'Me.DigitalIOCtrl1.Name = "DigitalIOCtrl1"
        'Me.DigitalIOCtrl1.Size = New System.Drawing.Size(255, 343)
        'Me.DigitalIOCtrl1.TabIndex = 5
        '
        'IO_frm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(274, 389)
        Me.ControlBox = False
        Me.Controls.Add(Me.hide_btn)
        'Me.Controls.Add(Me.DigitalIOCtrl1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Name = "IO_frm"
        Me.Text = "IO Control:"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents DigitalIOCtrl1 As dio_ctrl.DigitalIOCtrl
    Friend WithEvents hide_btn As System.Windows.Forms.Button
End Class

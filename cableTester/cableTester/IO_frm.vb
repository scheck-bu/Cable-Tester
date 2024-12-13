Public Class IO_frm
    Private Sub hide_btn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles hide_btn.Click
        Me.Hide()
    End Sub

    Public Sub addDIOCtrl(ByVal niPath As String, ByVal equipmentH As equipmentHandler.handler)
        If Not equipmentH.equipment Is Nothing Then
            For cntr = 0 To equipmentH.equipment.Length - 1
                If equipmentH.equipment(cntr).type = "DAQ" Then

                    ' create digital IO control from DAQ instrument
                    Me.DigitalIOCtrl1 = New dio_ctrl.DigitalIOCtrl(niPath, equipmentH.equipment(cntr).model, equipmentH.equipment(cntr).device)

                    Me.DigitalIOCtrl1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
                    Me.DigitalIOCtrl1.Location = New System.Drawing.Point(9, 9)
                    Me.DigitalIOCtrl1.Name = "DigitalIOCtrl1"
                    Me.DigitalIOCtrl1.Size = New System.Drawing.Size(255, 343)
                    Me.DigitalIOCtrl1.TabIndex = 5
                    Me.DigitalIOCtrl1.Visible = True

                    Me.Controls.Add(DigitalIOCtrl1)
                    Me.Refresh()

                    Dim numCtrls As Integer = Me.Controls.Count

                    Exit For
                End If
            Next
        End If
    End Sub

    Private Sub IO_frm_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        DigitalIOCtrl1.Refresh()
    End Sub
End Class
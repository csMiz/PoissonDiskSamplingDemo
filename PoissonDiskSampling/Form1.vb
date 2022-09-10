' Frank @ 10 Sept 2022
' Reference:
' Bridson, Robert. "Fast Poisson disk sampling in arbitrary dimensions." SIGGRAPH sketches10.1 (2007): 1.


Public Class Form1

    Public Rnd As New Random
    Public Bitmap As Bitmap = Nothing
    Public G As Graphics = Nothing

    Public PointList As New List(Of PointF)
    Public ActiveList As New List(Of Integer)

    Public PoissonDiskRadius As Single = 120.0F
    Public GridSize As Single = 1.0F
    Public CanvasGrid As Integer(,)
    Public GridWidth As Integer = 1
    Public GridHeight As Integer = 1

    Const POINT_RADIUS As Single = 10.0F

    ''' <summary>
    ''' new added point id
    ''' </summary>
    Public RenderContent1 As Integer = -1
    ''' <summary>
    ''' last draw point id
    ''' </summary>
    Public RenderContent2 As Integer = -1
    ''' <summary>
    ''' line from parent to child, id
    ''' </summary>
    Public RenderContent3 As New Point(-1, -1)

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Bitmap = New Bitmap(1200, 1000)
        G = Graphics.FromImage(Bitmap)
        PBox.Image = Bitmap
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        PoissonDiskRadius = CSng(InputBox("Poisson Disk Radius: ",, PoissonDiskRadius.ToString("0.0")))
        PoissonDiskSamplingStep0()
        PoissonDiskSamplingStep1()
        DrawScreenInit()
        DrawScreen()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If ActiveList.Count = 0 Then
            Label1.Text = "Finished!"
            Timer1.Stop()
            Return
        End If
        PoissonDiskSamplingStep2()
        DrawScreen()
    End Sub

    Public Sub DrawScreenInit()
        G.Clear(Color.Black)
        PBox.Invalidate()
    End Sub

    Public Sub DrawScreen()
        If RenderContent3.X <> -1 Then
            Dim ptFrom As PointF = PointList(RenderContent3.X)
            Dim ptTo As PointF = PointList(RenderContent3.Y)
            G.DrawLine(Pens.Gray, ptFrom, ptTo)
        End If
        If RenderContent2 <> -1 Then
            Dim pt As PointF = PointList(RenderContent2)
            G.FillEllipse(Brushes.White, New Rectangle(pt.X - POINT_RADIUS, pt.Y - POINT_RADIUS, 2 * POINT_RADIUS, 2 * POINT_RADIUS))
        End If
        If RenderContent1 <> -1 Then
            Dim pt As PointF = PointList(RenderContent1)
            G.FillEllipse(Brushes.Green, New Rectangle(pt.X - POINT_RADIUS, pt.Y - POINT_RADIUS, 2 * POINT_RADIUS, 2 * POINT_RADIUS))
        End If
        RenderContent3 = New Point(-1, -1)
        RenderContent2 = -1

        PBox.Invalidate()
        Label1.Text = "ActiveCount: " & vbCrLf & ActiveList.Count.ToString
    End Sub


    Public Sub PoissonDiskSamplingStep0()
        GridSize = PoissonDiskRadius / Math.Sqrt(2.0F)
        GridWidth = Math.Floor(1200.0F / GridSize) + 1
        GridHeight = Math.Floor(1000.0F / GridSize) + 1
        ReDim CanvasGrid(GridWidth - 1, GridHeight - 1)
        For i = 0 To GridWidth - 1
            For j = 0 To GridHeight - 1
                CanvasGrid(i, j) = -1
            Next
        Next
    End Sub

    Public Sub PoissonDiskSamplingStep1()
        PointList.Clear()
        ActiveList.Clear()
        PointList.Add(New PointF(100 + Rnd.NextDouble * 1000, 100 + Rnd.NextDouble * 800))
        Dim initialPoint As PointF = PointList(0)
        CanvasGrid(Math.Floor(initialPoint.X / GridSize), Math.Floor(initialPoint.Y / GridSize)) = 0
        ActiveList.Add(0)
        RenderContent1 = 0
        RenderContent2 = -1
        RenderContent3 = New Point(-1, 0)
    End Sub

    Public Sub PoissonDiskSamplingStep2()
        If ActiveList.Count = 0 Then Return
        ' use last one in active_list as target
        Dim listId As Integer = ActiveList.Count - 1
        Dim targetId As Integer = ActiveList(listId)
        Dim targetPt As PointF = PointList(targetId)

        Const K As Integer = 30

        For i = 0 To K - 1
            Dim r As Single = PoissonDiskRadius + Rnd.NextDouble * PoissonDiskRadius
            Dim theta As Single = Rnd.NextDouble * 2.0F * Math.PI
            Dim candidate As New PointF(Math.Cos(theta) * r + targetPt.X, -Math.Sin(theta) * r + targetPt.Y)
            Dim candidateGrid As New Point(Math.Floor(candidate.X / GridSize), Math.Floor(candidate.Y / GridSize))
            Dim valid As Boolean = False
            If candidate.X > 0 AndAlso candidate.X < 1200 AndAlso candidate.Y > 0 AndAlso candidate.Y < 1000 Then
                valid = True
                For m = candidateGrid.X - 2 To candidateGrid.X + 2
                    For n = candidateGrid.Y - 2 To candidateGrid.Y + 2
                        If m >= 0 AndAlso m < GridWidth AndAlso n >= 0 AndAlso n < GridHeight Then
                            If CanvasGrid(m, n) <> -1 Then
                                Dim cmpId As Integer = CanvasGrid(m, n)
                                Dim cmpPt As PointF = PointList(cmpId)
                                Dim dist As Single = Math.Sqrt(((cmpPt.X - candidate.X) ^ 2) + ((cmpPt.Y - candidate.Y) ^ 2))
                                If dist < PoissonDiskRadius Then
                                    valid = False
                                    GoTo LBL_AFTER_K
                                End If
                            End If
                        End If
                    Next
                Next
            End If
LBL_AFTER_K:
            If valid Then
                Dim candidateId As Integer = PointList.Count
                CanvasGrid(candidateGrid.X, candidateGrid.Y) = candidateId
                PointList.Add(candidate)
                ActiveList.Add(candidateId)
                RenderContent2 = RenderContent1
                RenderContent1 = candidateId
                RenderContent3 = New Point(targetId, candidateId)
                Return
            End If
        Next
        ' if K iterations all failed
        ActiveList.RemoveAt(listId)
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Timer1.Enabled = Not Timer1.Enabled
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Button2_Click(Nothing, Nothing)
    End Sub

End Class

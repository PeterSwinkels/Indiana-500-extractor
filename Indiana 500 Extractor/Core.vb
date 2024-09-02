'This module's imports and settings.
Option Compare Binary
Option Explicit On
Option Infer Off
Option Strict On

Imports System
Imports System.BitConverter
Imports System.Collections.Generic
Imports System.Convert
Imports System.Diagnostics
Imports System.Environment
Imports System.IO
Imports System.Linq

'This module contains this program core procedures.
Public Module CoreModule
   Private Const DECOMPRESSOR As String = "unlzss.exe"   'Defines the external decompression program.

   'This procedure is executed when this program is started.
   Public Sub Main()
      Try
         If GetCommandLineArgs().Count = 2 Then
            ExtractData(GetCommandLineArgs().Last)
         Else
            Console.WriteLine("Specify an input file.")
         End If
      Catch ExceptionO As Exception
         DisplayError(ExceptionO)
      End Try
   End Sub

   'This procedure starts the decompressor and passes the specified file to it
   Private Sub Decompress(CompressedFile As String)
      Try
         Dim ProcessO As Process = Process.Start(New ProcessStartInfo With {.FileName = DECOMPRESSOR, .Arguments = CompressedFile, .UseShellExecute = False, .RedirectStandardError = False, .RedirectStandardInput = False, .RedirectStandardOutput = False})  'This procedure launches the external decompression program.

         ProcessO.WaitForExit()
      Catch ExceptionO As Exception
         DisplayError(ExceptionO)
      End Try
   End Sub

   'This procedure displays any errors that occur.
   Private Sub DisplayError(ExceptionO As Exception)
      Try
         Console.Error.WriteLine($"ERROR: {ExceptionO.Message}")
         [Exit](0)
      Catch
         [Exit](0)
      End Try
   End Sub

   'This procedure extracts the data from the specified file.
   Private Sub ExtractData(InputFile As String)
      Try
         Dim Data() As Byte = {}
         Dim FirstOffset As New Integer
         Dim Offsets As New List(Of Integer)
         Dim OutputFile As String = Nothing

         With New BinaryReader(New MemoryStream(File.ReadAllBytes(InputFile)))
            FirstOffset = .ReadInt32()
            Offsets.Add(FirstOffset)
            Do While .BaseStream.Position + &H4% < FirstOffset
               Offsets.Add(.ReadInt32())
            Loop

            For Index As Integer = 0 To Offsets.Count - 1
               .BaseStream.Seek(Offsets(Index), SeekOrigin.Begin)
               Data = .ReadBytes(If(Index < Offsets.Count - 1, Offsets(Index + 1), CInt(.BaseStream.Length)) - Offsets(Index))
               OutputFile = Path.Combine(Path.GetDirectoryName(InputFile), $"{Offsets(Index)}_{Path.GetFileName(InputFile)}")
               File.WriteAllBytes(OutputFile, Data)
               Decompress(OutputFile)
            Next Index
         End With
      Catch ExceptionO As Exception
         DisplayError(ExceptionO)
      End Try
   End Sub

End Module

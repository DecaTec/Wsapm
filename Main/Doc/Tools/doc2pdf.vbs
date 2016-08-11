' doc2pdf: creates a PDF file from a doc/docx document
'
' Usage: doc2pdf <input file (doc or docx)> <export file (PDF)>
' If the second parameter is not specified, the PDF file is generated in the folder of the doc/docx file and has the same name as the doc/docx file.
'
' Examples:
' doc2pdf C:\Test\MyDoc.docx c:\Test2\MyPdf.pdf - creates a MyPdf.pdf file in C:\Test2
' doc2pdf C:\Test\MyDoc.docx - Creates a MyDoc.pdf file in C:\Test
'

Const WdDoNotSaveChanges = 0
Const WdFormatPDF = 17

Dim args
Set args = WScript.Arguments

Dim doc
Dim pdf

Call CheckArgs()
Call DOC2PDF(doc, pdf)

Set args = Nothing

Function CheckArgs()
  If args.Unnamed.Count = 0 Then
    WScript.Quit 1
  ElseIf args.Unnamed.Count = 1 Then
	doc = args.Unnamed(0)
	pdf = ""
 ElseIf args.Unnamed.Count = 2 Then
	doc = args.Unnamed(0)
	pdf = args.Unnamed(1)
 Else
	WScript.Quit 1
  End If
End Function

Function Doc2Pdf(fileDoc, filePdf)
  Dim fso
  Dim word
  Dim wordDocument
  Dim wordDocuments
  Dim folder

  Set fso = CreateObject("Scripting.FileSystemObject")
  Set word = CreateObject("Word.Application")
  Set wordDocuments = word.Documents

  fileDoc = fso.GetAbsolutePathName(fileDoc)
  folder = fso.GetParentFolderName(fileDoc)
    
  If Len(filePdf) = 0 Then
    filePdf = fso.GetBaseName(fileDoc) + ".pdf"
  Else
    If Not fso.FolderExists(fso.GetParentFolderName(filePdf)) Then
        fso.CreateFolder(fso.GetParentFolderName(filePdf))
    End If
  End If

  If Len(fso.GetParentFolderName(filePdf)) = 0 Then
    filePdf = folder + "\" + filePdf
  End If

  If fso.FileExists(filePdf) Then
    fso.DeleteFile(filePdf)
  End If

  Set wordDocument = wordDocuments.Open(fileDoc)
  wordDocument.SaveAs filePdf, WdFormatPDF
  wordDocument.Close WdDoNotSaveChanges
  word.Quit WdDoNotSaveChanges
  Set word = Nothing
  Set fso = Nothing
End Function
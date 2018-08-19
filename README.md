# GZIP

Многопоточный GZIP-компрессор/декомпрессор файлов.  
Использован паттерн "Consumer-Producer", реализована thread-safe очередь (аналог ConcurrentQueue).

Целевая платформа: .NET 3.5   

Использование: GZipTest.exe compress/decompress <input path> <output path>

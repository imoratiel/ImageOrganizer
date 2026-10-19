# ImageOrganizer

Aplicación de escritorio para Windows (WPF, .NET Core 3.1) que sirve para clasificar fotos rápidamente: muestra las imágenes de una carpeta una a una y permite enviar cada una a una subcarpeta con un solo clic.

## Funcionalidades

- **Clasificar en subcarpetas**: por cada subcarpeta de la carpeta escaneada aparece un botón; al pulsarlo, la imagen actual se mueve allí y se carga la siguiente.
- **Rotar** a izquierda o derecha (la rotación se guarda en el archivo original).
- **Saltar** imágenes: quedan marcadas como ignoradas y no vuelven a aparecer hasta pulsar *Reset Ignored Files in Folder*.
- **Deshacer** la última acción (mover, rotar o saltar).
- **Eliminar** la imagen actual o **abrirla** con el visor predeterminado de Windows.
- **Aviso de duplicados**: si una imagen tiene el mismo hash SHA-256 que la anterior, se muestra una notificación.
- **Orientación EXIF**: las fotos se muestran con la orientación correcta según sus metadatos.
- **Sobrescribir**: el interruptor *Overwrite* decide si se reemplaza un archivo que ya exista en la carpeta de destino.
- Recuerda las carpetas añadidas y vuelve a abrir la última al iniciar.

Formatos soportados: `.jpg`, `.jpeg`, `.jfif`, `.png`, `.bmp`, `.gif`.

## Requisitos

- Windows
- [.NET Core 3.1 SDK](https://dotnet.microsoft.com/download/dotnet/3.1)

## Compilar y ejecutar

```bash
dotnet build
dotnet run --project ImageOrganizer
```

También se puede abrir `ImageOrganizer.sln` con Visual Studio.

## Uso

1. Pulsa **Add Folder** y elige la carpeta con las fotos a clasificar. Esa carpeta debe contener las subcarpetas de destino.
2. Selecciona la carpeta en el desplegable y pulsa **Scan Folder**.
3. Para cada imagen, pulsa el botón de la subcarpeta de destino, o usa **Skip**, **Delete**, los botones de rotación o **Undo**.

En la parte inferior se ve el nombre del archivo actual y el progreso en la carpeta.

## Datos

El historial de acciones (carpetas, archivos ignorados y la pila de deshacer) se guarda en una base de datos SQLite en `%LOCALAPPDATA%\FileActionDB.db`. Para empezar de cero, basta con borrar ese archivo.

> ⚠️ **Eliminar** borra el archivo directamente (no va a la papelera) y no se puede deshacer. **Rotar** sobrescribe la imagen original; en `.bmp` y `.gif` la rotación no se guarda.

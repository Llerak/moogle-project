Moogle!
=====================

Este proyecto consiste en un motor de búsqueda construido con el lenguaje C# y el framework .NET que ofrece sugerencias de palabras y una búsqueda avanzada basada en la formulación TF-IDF y en la similitud del coseno. El motor de búsqueda también proporciona una corrección ortográfica mediante la distancia de Levenshtein para permitir búsquedas aunque se hayan escrito erróneamente las palabras clave, además proporciona herramientas como operadores para obtener un resultado más avanzado en la búsqueda. La búsqueda del motor devolverá resultados organizados en orden de relevancia, con un fragmento de texto y la parte más importante según la búsqueda del usuario.

Requisitos
-----
- .NET Framework 7.0

Uso
-----
Puedes usar el motor para realizar búsquedas usando una búsqueda libre o una sintaxis especial que incluya características avanzadas:

- Buscar una oración específica: "hola cómo estás"
- Buscar palabras específicas: cómo AND estás OR hola
- Buscar palabras clave con errores ortográficos: hiloa AND estass
- Hacer uso de los distintos operadores como son:
    - No-aparición(!): (!palabra) Para que el documento mostrado no contenga la palabra.
    - Aparición(^): (^palabra) Para que el documento mostrado contenga la palabra.
    - Importancia(*): (*palabra) Para darle más importancia a la palabra en la búsqueda. Vale destacar que mientras más * le ponga delante a una palabra, más importante será esta en la búsqueda.
    - Cercanía(~): (palabra1 ~ palabra2) Mientras más cercanas estén las palabras en el documento, más importancia tendrá el mismo.

Ejecución
-----
Luego de añadir los documentos a la carpeta Content, simplemente basta con ejecutar en la carpeta raíz del proyecto los siguientes comandos según el sistema donde lo esté ejecutando:
- Linux: make dev
- Windows: dotnet watch run --project MoogleServer

Lo siguiente es introducir lo que se desea buscar en la barra de búsqueda, la cual tiene una interfaz muy intuitiva.

# Control — laboratorio de componentes reutilizables en Windows Forms

![WinForms CI](https://github.com/kortCobein/control/actions/workflows/winforms-ci.yml/badge.svg)

Este repositorio está pensado como un laboratorio para aprender a crear **controles reutilizables en C# Windows Forms**: botones, paneles, labels, cajas de texto y componentes propios que puedan aparecer en el **Toolbox de Visual Studio**, arrastrarse al diseñador y conservar comportamiento, apariencia, propiedades y eventos.

No busca ser solamente una aplicación terminada. La prioridad es entender cómo construir una pequeña biblioteca de interfaz reutilizable y cómo funciona WinForms por debajo del diseñador.

## Stack actual

- C#
- Windows Forms
- .NET Framework 4.8.1
- Visual Studio 2022
- MSBuild
- GitHub Actions

Para trabajar con el proyecto conviene tener instalada la carga de trabajo **Desarrollo de escritorio de .NET** y el targeting/developer pack de **.NET Framework 4.8.1**.

## Qué se va a practicar

El proyecto irá creciendo alrededor de estos conceptos:

1. Herencia de controles nativos como `Button`, `Panel`, `Label` y `TextBox`.
2. Creación de `UserControl` cuando un componente necesite combinar varios controles.
3. Propiedades personalizadas visibles desde la ventana **Properties**.
4. Eventos y comportamiento reutilizable: hover, focus, click, validaciones, estados, etc.
5. Pintado personalizado con `OnPaint` cuando haga falta controlar la apariencia.
6. Atributos de diseño como `Category`, `Description`, `DefaultValue` y `Browsable`.
7. Controles que puedan compilarse y reutilizarse desde el **Toolbox**.
8. Separación entre apariencia, comportamiento y lógica de la aplicación.
9. Reutilización sin copiar y pegar código entre formularios.
10. Evolución posterior hacia una biblioteca de controles independiente.

## Ruta sugerida de aprendizaje

La idea es avanzar de lo simple a lo complejo:

```text
Control nativo
    ↓
Clase heredada
    ↓
Propiedades personalizadas
    ↓
Eventos / estados
    ↓
Pintado personalizado
    ↓
UserControl
    ↓
Componente reutilizable
    ↓
Biblioteca propia de controles
```

Un orden práctico para los primeros componentes sería:

```text
Controls/
├── Labels/
│   └── LabelUT.cs
├── Buttons/
│   └── ButtonUT.cs
├── Panels/
│   └── PanelUT.cs
├── Inputs/
│   └── TextBoxUT.cs
└── Composite/
    └── CardUT.cs
```

No es obligatorio crear toda esa estructura desde el principio. Se puede ir agregando conforme cada control tenga una razón real de existir.

## Primer patrón: heredar un control existente

Para crear un botón propio no es necesario empezar desde cero. Se puede heredar de `Button`:

```csharp
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace control.Controls.Buttons
{
    public class ButtonUT : Button
    {
        private Color hoverBackColor = Color.Teal;

        [Category("UT")]
        [Description("Color que usa el botón cuando el cursor está encima.")]
        public Color HoverBackColor
        {
            get => hoverBackColor;
            set => hoverBackColor = value;
        }

        public ButtonUT()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
        }

        protected override void OnMouseEnter(System.EventArgs e)
        {
            base.OnMouseEnter(e);
            BackColor = HoverBackColor;
        }
    }
}
```

Lo importante aquí no es el color. Lo importante es que la clase ya encapsula comportamiento. Cada formulario que use `ButtonUT` recibe ese comportamiento sin volver a programarlo.

## Cómo hacer que aparezca en el Toolbox

Para un control público que hereda de un control de WinForms:

1. Crear la clase dentro del proyecto.
2. Asegurarse de que la clase sea `public`.
3. Compilar el proyecto con **Build > Build Solution**.
4. Abrir un formulario en modo diseñador.
5. Revisar el **Toolbox**.

Visual Studio suele detectar los controles compilados del proyecto. Si no aparece automáticamente:

1. Clic derecho en el Toolbox.
2. **Choose Items...**
3. Buscar el ensamblado compilado del proyecto.
4. Seleccionar el control.

Después se podrá arrastrar al formulario como cualquier `Button`, `Panel` o `Label`.

## Propiedades que se configuran desde el diseñador

Uno de los objetivos centrales es que los controles no dependan de valores quemados. Las propiedades públicas pueden exponerse al diseñador:

```csharp
[Category("Apariencia")]
[Description("Radio visual utilizado por el control.")]
[DefaultValue(12)]
public int BorderRadius { get; set; } = 12;
```

Esto permite modificar el control desde la ventana **Properties** sin editar directamente su código.

## Cuándo usar cada enfoque

**Clase heredada de Button/Panel/Label/TextBox:** cuando se quiere conservar casi todo el funcionamiento del control original y agregar estilo o comportamiento.

**UserControl:** cuando un componente está formado por varios controles, por ejemplo una tarjeta con icono, título, descripción y botón.

**Control personalizado:** cuando se necesita controlar directamente el dibujo, medidas o comportamiento y un control nativo ya no es suficiente.

## Automatización incluida

El repositorio tiene el workflow:

```text
.github/workflows/winforms-ci.yml
```

Cada `push` y Pull Request contra `master` ejecuta una compilación limpia en Windows mediante MSBuild.

```text
push / pull request
        ↓
GitHub descarga el repositorio
        ↓
Configura MSBuild
        ↓
Compila control.csproj en Release
        ↓
✅ compiló
o
❌ hay un error
```

Si la compilación termina correctamente, GitHub guarda durante 14 días el contenido de `bin/Release` como artifact llamado:

```text
control-winforms-release
```

Esto sirve como segunda comprobación independiente de la computadora donde se esté programando.

## Ayuda mientras se programa

También se agregó un archivo `.editorconfig`. Visual Studio lo lee directamente y lo usa para mantener criterios de formato y mostrar sugerencias de C#.

Esto sí actúa durante el desarrollo local, a diferencia de GitHub Actions, que se ejecuta después de subir cambios.

## Flujo de trabajo recomendado

```text
Crear o modificar un control
        ↓
Probarlo desde el diseñador
        ↓
Compilar en Visual Studio
        ↓
Probar propiedades y eventos
        ↓
Commit
        ↓
Push
        ↓
GitHub Actions vuelve a compilarlo
```

Comandos básicos:

```bash
git add .
git commit -m "feat: agregar ButtonUT reutilizable"
git push
```

## Criterio para considerar terminado un control

Antes de dar por terminado un componente conviene comprobar que:

- se puede reutilizar en más de un formulario;
- no depende de un formulario específico;
- las propiedades importantes pueden configurarse;
- sus eventos siguen funcionando normalmente;
- aparece y puede utilizarse desde el diseñador;
- no duplica comportamiento que debería vivir en una clase base;
- el proyecto sigue compilando localmente y en GitHub Actions.

## Estado

El proyecto está en etapa inicial. La base actual es una aplicación Windows Forms sobre .NET Framework 4.8.1 y se irá convirtiendo progresivamente en un laboratorio de controles reutilizables.

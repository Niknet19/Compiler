# Персональный вариант:
Десятичные константы языка С/С++
```
const float id = + 123.123;
```
# Справка по текстовому редактору
## Пункты меню

- **Файл → Создать (Ctrl+N)**: Создает новую вкладку с пустым текстовым файлом. Имя файла по умолчанию — `NewFileN.txt`, где `N` — номер следующего файла.
- **Файл → Открыть (Ctrl+O)**: Открывает диалог для выбора текстового файла, который затем отображается в новой вкладке с именем файла.
- **Файл → Сохранить (Ctrl+S)**: Сохраняет содержимое текущей вкладки. Если файл уже имеет путь, сохраняет по нему; иначе открывает диалог "Сохранить как".
- **Файл → Сохранить как**: Открывает диалог для сохранения текущей вкладки под новым именем и в новом месте.
- **Файл → Выход**: Закрывает приложение с запросом сохранения всех измененных вкладок.
- **Правка → Отменить**: Отменяет последнее действие в текущей вкладке (например, удаление текста).
- **Правка → Повторить**: Повторяет отмененное действие в текущей вкладке.
- **Правка → Вырезать**: Вырезает выделенный текст из текущей вкладки в буфер обмена.
- **Правка → Копировать**: Копирует выделенный текст в буфер обмена.
- **Правка → Вставить**: Вставляет текст из буфера обмена в текущую вкладку.
- **Правка → Удалить**: Удаляет выделенный текст в текущей вкладке.
- **Правка → Выделить все**: Выделяет весь текст в текущей вкладке.
- **Текст → Размер шрифта → Увеличить**: Увеличивает размер шрифта текста во всех вкладках и области результатов на 2 пункта (до 72).
- **Текст → Размер шрифта → Уменьшить**: Уменьшает размер шрифта на 2 пункта (не менее 6).
- **Текст → Размер шрифта → Сбросить**: Сбрасывает размер шрифта до стандартного значения (14).
- **Пуск**: Пока не реализован, зарезервирован для будущих функций.
- **Справка → Справка**: Открывает этот документ с описанием функций приложения.
- **Справка → О программе**: Показывает информацию о программе (версия, автор).

## Область ввода текста

Область ввода текста расположена в центральной части окна и представлена вкладками. Каждая вкладка соответствует открытому файлу и содержит:

- **Номера строк**: Слева от текста отображаются номера строк, которые автоматически обновляются при добавлении или удалении строк. Прокрутка номеров синхронизирована с текстом.
- **Текстовый редактор**: Основная область для ввода и редактирования текста. Поддерживает многострочный ввод, табуляцию и прокрутку при превышении видимой области.
- **Закрытие вкладки**: Крестик `×` на заголовке вкладки позволяет закрыть её с запросом сохранения изменений, если они есть.

Перетаскивание файла из проводника в область ввода открывает его как новую вкладку. Размер шрифта текста можно изменять через меню "Текст".

## Область вывода результатов

Область вывода результатов находится в нижней части окна под разделителем. Она предназначена для отображения информации о работе программы (пока не используется активно). Основные особенности:

- **Только чтение**: Текст в этой области нельзя редактировать напрямую.
- **Прокрутка**: При большом объеме текста появляется полоса прокрутки.
- **Размер шрифта**: Регулируется через меню "Текст" синхронно с областью ввода.

Разделитель между областями ввода и вывода можно перетаскивать для изменения их размеров.

# Грамматика языка объявления вещественных констант

Этот документ описывает автоматную грамматику для языка, который позволяет объявлять переменные с типами `float` или `double` и присваивать им вещественные числа. Грамматика является регулярной, что означает, что она может быть реализована с помощью конечного автомата.

Особенности:
- Начинается с ключевого слова `const`.
- За ним следует тип: `float` или `double`.
- Затем идёт идентификатор (имя переменной), начинающийся с буквы.
- После идентификатора — вещественное число (с возможным знаком, дробной частью и экспонентой).
- Строка заканчивается точкой с запятой `;`.
- Между частями строки может быть произвольное количество пробелов (один значащий пробел минимум).

## Диаграмма состояний сканера

Ниже представлена диаграмма остояний сканера:

![Диаграмма](Compiler/icons/Parsing.svg)


## Описание Грамматики

### Терминальные символы (Vt)
```
Vt = {a, ... z,A, ... Z, 0, ... 9, "=", "+", "-", " ",.,;, const, float, double, letter, digit}
```
### Нетерминальные символы (Vn)
```
Vn = {<START>, <SPACE_AFTER_CONST>, <SPACE_AFTER_TYPE>, <TYPENAME>,<ID>,<IDREM>, <NUMBER>, <INTREM>, <REAL>, <REALREM>, <SEMICOLON>}
```
## Правила вывода
```
1) START -> "const" <SPACE_AFTER_CONST>
2) <SPACE_AFTER_CONST> -> " " <TYPENAME>
3) <TYPENAME> -> "float" <SPACE_AFTER_TYPE> | "double" <SPACE_AFTER_TYPE>
4)<SPACE_AFTER_TYPE> -> " " <ID>
5)<ID> -> letter <IDREM>
6)<IDREM> -> letter <IDREM> | digit <IDREM> | "=" <NUMBER>
7)<NUMBER> -> "+" <INTREM> | "-" <INTREM> | <INTREM>
8)<INTREM> = digit <INTREM> | <REAL> | digit <SEMICOLON>
9)<REAL> -> "." <REALREM>
10)<REALREM> -> digit <REALREM> | digit <SEMICOLON>
11) <SEMICOLON> -> ";"

letter ->  a | b | ... | z | A | B | ... | Z
digit -> 0 | 1 | ... | 9
```

## Классификация грамматики
- Грамматика является автоматной (регулярной), так как все правила соответствуют формату \( A -> aB \) или \( A -> a \).

## Примеры допустимых строк
Язык поддерживает строки вида:
- `const float x = 123.45;`
- `const double yyy = -.789 ;`
- `const float z = 1123;`

## Граф конечного автомата для данного языка
![Graph](https://github.com/user-attachments/assets/39d1c3bf-54b0-4e82-830d-92b7831422ca)

## Тестовые примеры
![Снимок экрана 2025-04-15 185949](https://github.com/user-attachments/assets/1b0bc4b0-f319-43d2-aea6-57f4ff705593)
![Снимок экрана 2025-04-15 190015](https://github.com/user-attachments/assets/c60c69b9-fd5e-4ab2-b3ec-1355afd52435)
![Снимок экрана 2025-04-15 190123](https://github.com/user-attachments/assets/9c51fc07-ce24-4ddc-83bd-71e46cb63e75)
![Снимок экрана 2025-04-15 190422](https://github.com/user-attachments/assets/590d6213-d5fd-4400-931e-6fec59cd59d0)

# Описание регулярных выражений
## 1.Российский телефонный номер

### Шаблон: 
- `(\d[ -]?){6}\d`

### Описание: 
Соответствует 7-значному российскому телефонному номеру. Между цифрами (кроме последней) могут быть пробелы или дефисы.
Примеры соответствия:
- 1234567
- 123-456-7
- 123 456 7

### Тестовый пример:
```string text = "Позвоните по номеру 1234567 или 123-456-7 или 123 456 7!";```
### Вывод
```
Найдено: 1234567 в позиции 20
Найдено: 123-456-7 в позиции 32
Найдено: 123 456 7 в позиции 46
```
![image](https://github.com/user-attachments/assets/0d52b156-974e-428e-8acb-03df31f04386)


## 2.Код ISBN-13

### Шаблон: 
- `(ISBN[- ]?13)?[- ]?(?=\d{13}$)\d{1,5}([- ]?)\d{1,7}\1\d{1,6}\1\d`

### Описание: 
Соответствует международному стандартному книжному номеру ISBN-13 (13 цифр). Поддерживает необязательный префикс ISBN-13 и разделители (пробелы или дефисы).

Примеры соответствия:
- 9780306406157
- 978-0-306-40615-7
- ISBN-13: 9780306406157

### Тестовый пример:
```string text = "Книги: ISBN-13: 9780306406157, 978-0-306-40615-7, ISBN-13: 978-1-56619-909-4"```
### Вывод
```
Найдено: 'ISBN-13: 9780306406157' на позиции 7
Найдено: '978-0-306-40615-7' на позиции 31
Найдено: 'ISBN-13: 978-1-56619-909-4' на позиции 50
```
![image](https://github.com/user-attachments/assets/f5156868-64de-43fa-9e0e-b0965ef7680f)


## 3.Адрес электронной почты

### Шаблон: 
- `[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}`

### Описание: 
Соответствует адресам электронной почты в стандартном формате: локальная часть, символ @, домен и доменная зона (2+ буквы).


Примеры соответствия:
- user@example.com
- john.doe@sub.domain.org



### Тестовый пример:
```string text = "Контакты: user@example.com, john.doe@sub.domain.org, invalid@com";```
### Вывод
```
Найдено: user@example.com в позиции 10
Найдено: john.doe@sub.domain.org в позиции 28
```
![image](https://github.com/user-attachments/assets/74177e8b-b4f8-40dd-b78c-e7f83a813770)

## Граф автомата
![Regex](https://github.com/user-attachments/assets/c8766c9d-c0aa-45ac-9422-0b99cb2e8054)




# Hackathon Strategy | C# Course

Стратегия основана на генетическом алгоритме

### Инициализация популяции:

- **Венгерский алгоритм**: начальное решение генерируется с помощью Венгерского алгоритма, который обеспечивает качественное распределение на старте. 
- **Случайные решения**: остальные особи популяции создаются случайным образом, что добавляет разнообразие и помогает исследовать разные области решений. 

### Эволюция через поколения:

- **Оценка фитнеса**: каждое решение оценивается по средней гармонической удовлетворённости всех команд. Чем выше значение, тем лучше.
- **Отбор родителей**: используется турнирный отбор, при котором из случайного поднабора выбираются лучшие решения для создания потомков.
- **Кроссовер**: применяется одноточечный кроссовер для комбинирования генов родителей, создавая новые решения.
- **Мутация**: с определённой вероятностью происходит случайная перестановка джунов между тимлидами, что помогает избежать локальных максимумов.
- **Формирование новой популяции**: созданные потомки добавляются в новую популяцию вместе с хорошими решениями, обеспечивая сохранение лучших результатов.

## Улучшение стратегии

Стратегия может быть улучшена путём изменения параметров генетического алгоритма, таких как размер популяции, количество поколений, вероятность мутации и вероятность кроссовера.

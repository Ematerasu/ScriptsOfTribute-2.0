# 🗂️ TODO – Tales of Tribute Unity (Singleplayer)

## 🔥 Wysoki priorytet (krytyczne)

- [x] **End Game Panel** – zaimplementować panel końca gry (`EndGameState`, `InitialSeed`, `CompletedActions`, itd.)
- [x] **Animacja aktywacji agentów** – animacja nie może być opóźniona `delayem`; jawna kolejność z `CompletedActionProcessor`
- [x] **Hit effect** – efekt trafienia / utraty HP agenta, np. miganie lub cząsteczki
- [x] **Widoczne combosy** – pokazać jawnie odpalone Combo2/3/4 w turze gracza
- [x] **HP na przybliżeniu karty agenta** – brak widocznego stanu zdrowia przy podglądzie
- [x] **UI podglądu stosów** – kliknięcie rewersu otwiera popup z listą kart w Draw/Cooldown/PlayedPile
- [x] **Poprawna rotacja okręgu patrona** – np. 0° → 90° → 180° przeciwnie do wskazówek zegara
- [x] **Lepsze animacje kart przy ich tworzeniu** - np. gdy patron tworzy karte to niech ona idzie od patrona do kupki z kartami
- [x] **6 agentów nachodzi na played pile** - ogarnąć lepsze ustawienie tych agentów

## ✅ Średni priorytet (ważne ale nie blokujące)

- [x] **Animacja agenta** – zamiast texture outline dodać efekt jak przy podświetleniu patronów
- [x] **Tooltipy** – przesunąć sprite’y w prawo (lepsza czytelność), dodać do CardLookup
- [ ] **Przyciski debugowe** – Cofanie ruchów?
- [x] **Tryb DEBUG** – Pełny podgląd do kart przeciwnika, logi bota
- [x] **Patron Calls notifier** – zeton z liczba patron calli dostepnych
- [ ] **Contract agent próbuje wrócic do cooldown** - powinien isc od razu do tawerny po wyjebce, a najpierw ustawiamy ze wraca do cooldown
- [ ] **Napisać własnego, lepszego bota** - Heura wzmocniona sieciami
- [x] **Obsługa gRPC botów**
- [ ] **Poprawne UI prefaby kart** - dla każdego decku trzeba by przygotować
- [ ] **Combo panel** - niech nie tylko pokazuje combosy ale liczbę kart zagranych z danego decka

## 🧪 Techniczne (setup, multiplayer, tutorial)

- [x] **Start menu + menu setupu gry** – wybór botów, seed, strona, decki
- [x] **Audio** – dzwieki kart, patronów, muzyka
- [ ] **Multiplayer fundamenty** – synchronizacja stanu, wybór modelu (host/client, peer-to-peer)
- [ ] **Tutorial scene** – scena testowa do nauki rozgrywki
- [x] **Auto move** - ustaw że bot od razu się rusza sam, bez klikania guzika
- [x] **Delay na przycisk Choice UI** - bo mozna przypadkowo kliknac instant

## 📦 Niższy priorytet (QoL, polish)

- [x] **Animacje kart w ręce** – lepsze przemieszczanie się kart po zagraniu
- [ ] **Animacja kontraktów** – kontrakty trafiają od razu do tawerny (animacja + efekt)
- [ ] **Podświetlenie możliwych ruchów** – highlight dostępnych kart
- [ ] **Pasek statusu gry** – aktualna tura

## 🧠 Sugestie (dodatkowe)

- [x] **Auto Play dla AI** – opcje typu: „Play Turn”, „Play Until GameEnd”
- [ ] **Logger rozgrywek** – rejestrowanie ruchów do pliku .json (AI/ML ready)
- [ ] **Tryb sandboxowy / edytor GameState** – ręczne ustawianie stanu gry
- [ ] **Skróty klawiszowe** – End Turn, AI Move, toggle debug overlay

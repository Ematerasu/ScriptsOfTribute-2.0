# 🗂️ TODO – Tales of Tribute Unity (Singleplayer)

## 🔥 Wysoki priorytet (krytyczne)

- [x] **End Game Panel** – zaimplementować panel końca gry (`EndGameState`, `InitialSeed`, `CompletedActions`, itd.)
- [x] **Animacja aktywacji agentów** – animacja nie może być opóźniona `delayem`; jawna kolejność z `CompletedActionProcessor`
- [x] **Hit effect** – efekt trafienia / utraty HP agenta, np. miganie lub cząsteczki
- [ ] **Widoczne combosy** – pokazać jawnie odpalone Combo2/3/4 w turze gracza
- [x] **HP na przybliżeniu karty agenta** – brak widocznego stanu zdrowia przy podglądzie
- [x] **UI podglądu stosów** – kliknięcie rewersu otwiera popup z listą kart w Draw/Cooldown/PlayedPile
- [x] **Poprawna rotacja okręgu patrona** – np. 0° → 90° → 180° przeciwnie do wskazówek zegara
- [x] **Lepsze animacje kart przy ich tworzeniu** - np. gdy patron tworzy karte to niech ona idzie od patrona do kupki z kartami

## ✅ Średni priorytet (ważne ale nie blokujące)

- [ ] **Animacja agenta** – zamiast texture outline dodać efekt jak przy podświetleniu patronów
- [ ] **Tooltipy** – przesunąć sprite’y w prawo (lepsza czytelność)
- [ ] **PPM tooltip** – kliknięcie PPM ponownie powinno tooltip zamknąć
- [ ] **Przyciski debugowe** – Cofanie ruchów?
- [ ] **Tryb DEBUG** – Pełny podgląd do kart przeciwnika, logi bota
- [x] **Patron Calls notifier** – zeton z liczba patron calli dostepnych

## 🧪 Techniczne (setup, multiplayer, tutorial)

- [ ] **Start menu + menu setupu gry** – wybór botów, seed, strona, decki
- [ ] **Multiplayer fundamenty** – synchronizacja stanu, wybór modelu (host/client, peer-to-peer)
- [ ] **Tutorial scene** – scena testowa do nauki rozgrywki

## 📦 Niższy priorytet (QoL, polish)

- [ ] **Animacje kart w ręce** – lepsze przemieszczanie się kart po zagraniu
- [ ] **Animacja kontraktów** – kontrakty trafiają od razu do tawerny (animacja + efekt)
- [ ] **Podświetlenie możliwych ruchów** – highlight dostępnych kart
- [ ] **Pasek statusu gry** – aktualna tura, przewaga prestiżu, przychylność patronów

## 🧠 Sugestie (dodatkowe)

- [x] **Auto Play dla AI** – opcje typu: „Play Turn”, „Play Until GameEnd”
- [ ] **Logger rozgrywek** – rejestrowanie ruchów do pliku .json (AI/ML ready)
- [ ] **Tryb sandboxowy / edytor GameState** – ręczne ustawianie stanu gry
- [ ] **Skróty klawiszowe** – End Turn, AI Move, toggle debug overlay

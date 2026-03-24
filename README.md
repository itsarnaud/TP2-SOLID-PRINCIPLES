# Sprint 1 — Principes SOLID

## Contexte narratif

> L'hotel "Le Mas des Oliviers", situe dans l'Herault, vient d'ouvrir ses portes.
> Un developpeur junior a ete charge de creer le systeme de reservation.
> Le code fonctionne — les reservations se creent, les factures se generent, le menage est planifie.
> Mais le code est fragile : chaque modification risque de casser quelque chose d'inattendu.
>
> Votre mission : identifier les violations des principes SOLID et refactorer le code
> pour le rendre maintenable, extensible et testable.

## Donnees de test

| Chambre | Type     | Capacite | Prix/nuit |
|---------|----------|----------|-----------|
| 101     | Standard | 2 pers.  | 80 EUR    |
| 102     | Standard | 2 pers.  | 80 EUR    |
| 201     | Suite    | 2 pers.  | 200 EUR   |
| 301     | Familiale| 4 pers.  | 120 EUR   |

| Reservation     | Chambre | Dates          | Personnes |
|-----------------|---------|----------------|-----------|
| Alice Martin    | 101     | 15/06 -> 18/06 | 2         |
| Bob Dupont      | 201     | 15/06 -> 22/06 | 2         |
| Famille Durand  | 301     | 20/06 -> 25/06 | 4         |

## Instructions

### Prerequis

- .NET 8 SDK
- Un editeur de code (VS Code, Rider, Visual Studio)

### Lancer le projet

```bash
cd src/HotelReservation
dotnet run
```

Le programme execute tous les scenarios de test et affiche les resultats dans la console.

**Important** : votre code refactore doit produire **exactement le meme output** que le code de depart.

---

## Exercices

### Exercice 1 — SRP (Single Responsibility Principle)

**Rappel** : "Un module ne devrait etre responsable que devant un seul acteur" (Robert C. Martin).
Ce n'est PAS "une classe fait une seule chose". C'est "un seul decideur peut demander des changements sur cette classe".

#### 1.1 Melange de niveaux de responsabilite

Fichier : `Services/ReservationService.cs`

Cette classe melange trois types de preoccupations :
- **Infrastructure** : acces direct aux donnees, logging
- **Business** : verification de disponibilite, calcul de prix, validation
- **Applicatif** : orchestration du workflow

**A faire** : Separer en 3 couches distinctes :
1. Un **repository** (infrastructure) pour l'acces aux donnees
2. Un **domain service** (business) pour les regles metier
3. Un **application service** (applicatif) pour l'orchestration

#### 1.2 Melange de niveaux d'abstraction

Fichier : `Services/CheckInService.cs`

La methode `ProcessCheckIn` melange des operations de haut niveau (regles metier)
avec des details de bas niveau (manipulation de cache, notifications directes).

**A faire** : Refactorer pour que la methode principale ne contienne que des appels
de haut niveau. Les details sont delegues a des methodes ou collaborateurs dedies.

#### 1.3 Plusieurs acteurs, une seule classe (1 point)

Fichier : `Models/Reservation.cs`

La classe `Reservation` sert **trois acteurs** differents :
- Le **receptionniste** : `Cancel()` — regles d'annulation
- Le **comptable** : `CalculateTotal()`, `GenerateInvoiceLine()` — tarification
- La **gouvernante** : `GetLinenChangeDays()` — planning de menage


**A faire** : Extraire :
- `BillingCalculator` 
- `HousekeepingScheduler` 
- `Reservation` ne garde que les donnees et le cycle de vie 

**Dans votre justification, nommez explicitement les acteurs.**

---

### Exercice 2 — OCP (Open/Closed Principle)

**Rappel** : Ouvert a l'extension, ferme a la modification.

#### 2.1 Identifier les bons exemples

Le code contient deja des exemples **conformes** a OCP. Identifiez-les et expliquez
pourquoi ils respectent OCP :

- `Events/ReservationEventDispatcher.cs` — quel pattern ?
- `Interfaces/IPriceCalculator.cs` + `SeasonalSurchargeDecorator.cs` — quel pattern ?
- `Interfaces/ICleaningPolicy.cs` + implementations — quel pattern ?

#### 2.2 Corriger l'exemple mal fait

Fichier : `Services/CancellationService.cs`

Le `switch/case` sur `CancellationPolicy` viole OCP : ajouter une politique
"SuperFlexible" oblige a modifier cette classe.

**A faire** : Extraire une interface `ICancellationPolicy` avec une implementation
par politique (`FlexiblePolicy`, `ModeratePolicy`, `StrictPolicy`, `NonRefundablePolicy`).
Le `CancellationService` recoit la bonne politique par injection.

---

### Exercice 3 — LSP (Liskov Substitution Principle)

**Rappel** : Une implementation doit pouvoir remplacer son type parent sans casser
le comportement attendu par le code appelant.

#### 3.1 Implementation qui throw

Fichiers : `Interfaces/ICancellable.cs`, `FlexibleReservation.cs`, `NonRefundableReservation.cs`

`NonRefundableReservation` implemente `ICancellable` mais throw sur `Cancel()`.
Le code appelant qui utilise `ICancellable.Cancel()` explose au runtime.

**A faire** : Revoir la hierarchie d'interfaces. Separer :
- `IReservation` (base, sans Cancel)
- `ICancellableReservation` (avec Cancel, seulement pour les reservations annulables)

L'objectif : **le compilateur empeche l'appel invalide**, pas une exception au runtime.

#### 3.2 Violation semantique

Fichier : `Repositories/CachedRoomRepository.cs`

Le `CachedRoomRepository` ne respecte pas le contrat de `GetAvailableRooms` :
il retourne des donnees potentiellement perimees et ignore les parametres de date.
De plus, `Save()` n'invalide pas le cache.

**A faire** : Corriger pour respecter le contrat comportemental :
- `GetAvailableRooms` doit deleguer au repository interne pour des donnees fraiches
- `Save` doit invalider le cache

---

### Exercice 4 — ISP (Interface Segregation Principle)

Les violations ISP sont presentes dans le code pour enrichir votre comprehension,
mais la notation se concentre sur SRP, OCP, LSP et DIP.

Violations a identifier et corriger si possible :
- `Repositories/IReservationRepository.cs` : 9 methodes, alors que chaque consommateur
  n'en utilise que 1 a 3
- `Services/InvoiceGenerator.cs` : prend une `Reservation` complete mais n'utilise que 5 champs
- `Interfaces/INotificationService.cs` : 4 methodes, chaque consommateur n'en utilise qu'une

---

### Exercice 5 — DIP (Dependency Inversion Principle)

**Rappel** : Les modules de haut niveau ne doivent pas dependre des modules de bas niveau.
Les deux doivent dependre d'abstractions. **L'interface appartient au consommateur, pas au fournisseur.**

#### 5.1 Service metier couple a l'infrastructure

Fichier : `Services/BookingService.cs`

`BookingService` instancie directement `InMemoryReservationStore` et `FileLogger`.
Impossible de changer le stockage ou le logging sans modifier cette classe.

**A faire** :
1. Definir `IReservationRepository` et `ILogger` **dans le meme namespace que le service metier**
2. Les implementations d'infrastructure implementent ces interfaces
3. `BookingService` recoit les abstractions par constructeur

#### 5.2 Module metier couple au module technique

Fichier : `Services/HousekeepingService.cs`

Le module Housekeeping (metier) depend directement de `EmailSender` (technique).

**A faire** :
1. Definir `ICleaningNotifier` **dans le module Housekeeping/Domain**
2. Creer `EmailCleaningNotifier` comme **Adapter** dans Infrastructure
3. `HousekeepingService` ne connait que `ICleaningNotifier`

Le schema de dependance doit etre :
```
HousekeepingService -> ICleaningNotifier (dans Domain)
                              ^
               EmailCleaningNotifier (dans Infrastructure)
                              |
                         EmailSender (dans Infrastructure)
```

---

### Exercice 6 — Justification ecrite

Redigez **un paragraphe par principe SOLID** (5 paragraphes au total) expliquant :
- Le probleme identifie dans le code de depart
- La solution appliquee
- Pourquoi cela ameliore la maintenabilite

**Pour SRP, nommez explicitement les acteurs**.

---

## Livrables

1. Le code refactore dans `src/HotelReservation/` (doit compiler et produire le meme output)
2. Un fichier `JUSTIFICATION.md` avec vos choix

## Rappel

> Les patterns emergent des principes. Ne les appliquez pas mecaniquement.
> Comprenez d'abord le probleme, puis la solution viendra naturellement.

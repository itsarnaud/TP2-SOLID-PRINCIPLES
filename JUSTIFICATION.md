# Justification

## SRP

Le principe SRP stipule qu'une classe ou un module doit avoir une seule raison de changer, c'est-à-dire qu'il ne doit répondre qu'à un seul "acteur" (profil métier).

**Problème identifié** :

- Dans `ReservationService`, nous avions un mélange de responsabilités : l'accès aux données (Infrastructure), les règles de métier (Business) et la coordination (Applicatif).
- Dans `CheckInService`, la méthode principale mélangeait des notions de haut niveau (règles) et des appels très techniques de bas niveau, ce qui rendait le code illisible.
- Dans `Reservation.cs`, la classe répondait directement aux besoins de **trois acteurs différents** : le **Réceptionniste** (annulation de la réservation), le **Comptable** (règles de facturation et de TVA), et la **Gouvernante** (planning de changement des draps).

**Solution appliquée** :

- `ReservationService` a été découpé. Il délègue l'accès aux données à un `Repository`, et délègue les calculs métier à un `ReservationDomainService`.
- Dans `CheckInService`, les détails techniques ont été isolés dans des sous-méthodes pour maintenir la méthode principale à un seul niveau d'abstraction.
- Depuis le modèle `Reservation`, on a extrait `BillingCalculator` pour le comptable et `HousekeepingScheduler` pour la gouvernante, laissant la classe `Reservation` avec uniquement ses propres données et son cycle de vie.

**Bénéfice :**
Si le comptable demande une modification du taux de TVA, on ne risque plus de casser ou parasiter le code relatif au planning de ménage de la gouvernante. Chaque besoin métier a un endroit précis où il est pris en charge, ce qui limite fortement les effets de bord indésirables.

## OCP

Le principe OCP stipule qu'une entité logicielle doit être ouverte à l'extension (facile à enrichir) mais fermée à la modification (sans altérer le code existant).

**Problème identifié et exemples conformes :**
Dans le code fourni, plusieurs implémentations respectent déjà ce principe grâce à des Design Patterns :

- `ReservationEventDispatcher` utilise le pattern **Observer**. Ajouter une nouvelle action lors d'une réservation (ex: envoi de SMS) nécessite seulement de créer un nouveau `IReservationEventHandler`, sans modifier le dispatcher.
- `IPriceCalculator` et `SeasonalSurchargeDecorator` utilise le pattern **Decorator**. On peut empiler de nouvelles règles de prix (taxes, réductions) autour du calcul de base, sans jamais modifier le calculateur initial.
- `ICleaningPolicy` utilise le pattern **Strategy**. Ajouter une nouvelle règle de ménage (ex: nettoyage "Eco") demande juste de créer une nouvelle classe implémentant l'interface, évitant ainsi un grand `switch/case` centralisé.

À l'inverse, `CancellationService` violait ce principe : le calcul des frais d'annulation utilisait un `switch/case` sur l'énumération `CancellationPolicy`. Ajouter une nouvelle politique d'annulation obligeait à modifier cette classe.

**Solution appliquée :**
Nous avons refactoré `CancellationService` en appliquant le pattern **Strategy**. Nous avons extrait une interface `ICancellationPolicy` avec une méthode de calcul. Chaque règle d'annulation (`FlexiblePolicy`, `StrictPolicy`, etc.) possède désormais sa propre classe implémentant cette interface. `CancellationService` reçoit la bonne interface de l'extérieur sans avoir à connaître les détails internes de chaque règle.

## LSP (Liskov Substitution Principle)

**Problèmes identifiés :**

- **Violation technique** : La classe `NonRefundableReservation` implémentait l'interface `ICancellable` mais levait une exception `InvalidOperationException` lors de l'appel à `Cancel()`. N'importe quel code appelant `Cancel()` sur une politique stricte explosait au runtime.
- **Violation sémantique** : Le `CachedRoomRepository` prétendait rechercher des chambres disponibles entre deux dates (`GetAvailableRooms`) mais ignorait les paramètres de date. De plus, sa méthode `Save` n'invalidait pas le cache, renvoyant des données potentiellement périmées.

**Solutions appliquées :**

- **Correction technique** : J'ai revu la hiérarchie d'interfaces. J'ai créé une interface de base `IReservation` (sans la méthode Cancel) pour `NonRefundableReservation`, et une sous-interface `ICancellableReservation` pour la `FlexibleReservation`. Le compilateur empêche désormais l'erreur car la méthode n'existe plus sur l'objet strict.
- **Correction sémantique** : J'ai modifié `CachedRoomRepository` pour déléguer les requêtes complexes sur dates au dépôt interne (`_inner.GetAvailableRooms`), et J'ai forcé le retrait d'une chambre du cache lors de sa sauvegarde (`_cache.Remove`).

**Bénéfice :**
Les développeurs peuvent utiliser le polymorphisme en toute confiance. Si une fonction demande un `IReservation`, on sait formellement qu'on ne risque pas de crasher ou d'avoir des comportements malicieux cachés à l'intérieur d'une de ses dérivées.

## ISP (Interface Segregation Principle)

Le principe de ségrégation des interfaces (ISP) stipule qu'aucun client ne devrait être forcé de dépendre de méthodes qu'il n'utilise pas. Il vaut mieux avoir plusieurs petites interfaces spécifiques qu'une seule "grosse" interface généraliste.

**Problèmes identifiés :**

- `INotificationService` forçait toute classe d'envoi à supporter l'email, le SMS, le Push, et Slack, même si cette classe ne servait (par exemple) qu'à envoyer des SMS.
- `IReservationRepository` était une interface monolithique qui mélangeait la lecture, l'écriture et les statistiques. Un service de facturation voyait qu'il était techniquement possible de supprimer une réservation.
- `InvoiceGenerator` dépendait de l'objet complet `Reservation`, le rendant vulnérable aux changements de statut qui n'ont rien à voir avec la facturation.

**Solutions appliquées :**

- `INotificationService` a été scindé en 4 petites interfaces (une par médium).
- `IReservationRepository` a été découpé selon le pattern CQRS (ReadRepository, WriteRepository, StatsRepository). Le `ReservationService` dépend désormais de la bonne partie du contrat (Read pour la validation, Write pour la sauvegarde).
- `InvoiceGenerator` prend désormais une interface `IBillableStay` plus légère qui ne contient que les informations tarifaires et date d'une réservation.

**Bénéfice (Maintenabilité) :**
La sécurité et la lisibilité sont accrues : nous appliquons le least privilege. Quand un développeur demande une interface précise, l'IDE (IntelliSense) ne lui propose plus de méthodes hors de son champ de compétence, réduisant considérablement la surface des bugs.

## DIP (Dependency Inversion Principle)

Le principe d'inversion des dépendances stipule que les modules de haut niveau (règles métier) ne doivent pas dépendre des modules de bas niveau (infrastructure, base de données, mails), mais que tous deux doivent dépendre d'abstractions définies par le haut niveau.

**Problèmes identifiés :**

- `BookingService` faisait des appels directs vers `FileLogger` et `InMemoryReservationStore` via des mots-clés `new`. Une modification de base de données aurait nécessité de réécrire la logique métier.
- `HousekeepingService` sollicitait en dur `EmailSender`, rendant impossible l'adaptation à un envoi par SMS sans modifier les algorithmes de la gouvernante.

**Solutions appliquées :**

- Dans chaque service métier, nous avons défini les contrats dont il avait besoin (`ILogger`, `IReservationRepository`, `ICleaningNotifier`) et demandé ces dépendances par le constructeur.
- Nous avons créé un pattern Adapter dans l'infrastructure : `EmailCleaningNotifier` transforme l'appel abstrait du métier en un véritable envoi d'email via l'ancienne librairie.

**Bénéfice (Maintenabilité) :**
L'architecture devient "Plug and Play". Le cœur de l'application est préservé des changements technologiques. Demain, on peut passer d'une base SQL Server à MongoDB ou envoyer des alertes WhatsApp au lieu d'emails en créant simplement une nouvelle classe dans le dossier de l'infrastructure, sans jamais retoucher ni recompiler le code critique de la gestion des réservations.

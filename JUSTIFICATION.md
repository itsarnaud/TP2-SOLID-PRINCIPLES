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

**Bénéfice (Maintenabilité)** :
Si le comptable demande une modification du taux de TVA, on ne risque plus de casser ou parasiter le code relatif au planning de ménage de la gouvernante. Chaque besoin métier a un endroit précis où il est pris en charge, ce qui limite fortement les effets de bord indésirables.

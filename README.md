# BlindTest - Implémentation personnelle

## Contexte

Projet réalisé dans le cadre du projet d'été des DIIAGE.

Ce dépôt contient uniquement mon implémentation personnelle réalisée pendant ce projet.  
Les énoncés, ressources pédagogiques et tests fournis par l'école ne sont pas inclus dans ce dépôt.

## Description du projet

Ce projet consiste en la mise en place d'une gestion d'entités liées à une application de type BlindTest.

L'objectif principal était de manipuler des objets représentant des artistes, des albums et des morceaux, tout en respectant une logique de gestion des relations entre ces différentes entités.

## Fonctionnalités implémentées

### ArtistManager

Gestion des artistes :

- Ajout d'un artiste
- Suppression d'un artiste
- Mise à jour d'un artiste
- Recherche d'un artiste par son identifiant externe

### Gestion des albums

Gestion des albums associés aux artistes :

- Ajout d'un album à un artiste
- Association entre un album et son artiste
- Gestion des albums inexistants

### Gestion des tracks

Gestion des morceaux associés aux albums :

- Ajout d'une track dans un album
- Suppression d'une track d'un album
- Association entre une track et son album
- Gestion des erreurs lors des opérations

## Structure du projet
BlindTest
│
├── Entities
│ ├── Artist.cs
│ ├── Album.cs
│ ├── Track.cs
│ └── ArtistManager.cs
│
└── ...
## Technologies utilisées

- C#
- .NET
- Git
- Azure DevOps

## Remarques

Les tests unitaires utilisés pendant le développement proviennent du cadre pédagogique DIIAGE et ne sont volontairement pas présents dans ce dépôt.

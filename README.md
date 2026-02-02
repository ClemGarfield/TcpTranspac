# TcpTranspac

TcpTranspac est un protocole de transport binaire fiable, conçu pour les échanges internes d’un système d’information distribué.  
Il fournit un mécanisme de framing robuste, un en-tête structuré, un contrôle d’intégrité CRC64 et une API .NET simple pour construire des clients et serveurs TCP performants.

Ce projet est développé et maintenu par **C.H. Software**.

---

## Objectifs

TcpTranspac vise à offrir :

- un protocole interne maîtrisé, stable et performant ;
- un framing clair (STX/ETX) avec un en-tête binaire strict ;
- un contrôle d’intégrité complet (CRC64 sur header + payload + trame finale) ;
- une API .NET moderne pour envoyer et recevoir des messages typés ;
- une séparation nette entre transport et métier (payload libre : JSON, MessagePack, Protobuf, binaire, chiffré, etc.) ;
- une base solide pour construire un SI distribué, des microservices internes ou des modules industriels.

---

## Structure du protocole

Une trame TcpTranspac est composée de :

1. **STX** (1 octet)
2. **Header binaire** (46 octets)
3. **Payload** (0..N octets)
4. **ETX** (1 octet)
5. **CRC final** (8 octets)

Le protocole ne fait aucune hypothèse sur le contenu du payload.  
Il transporte simplement un tableau d’octets.

---

## Fonctionnalités principales

- Encodage/décodage complet des trames
- Gestion automatique du CRC64
- API client TCP asynchrone
- API serveur TCP multi‑clients
- Router interne basé sur le `MessageType`
- Gestion des réponses (pattern request/response)
- Extensible et indépendant du format métier

---

## Packages NuGet

Le projet est découpé en trois packages :

| Package | Description |
|--------|-------------|
| `TcpTranspac.Protocol` | Structures, framing, CRC, encodeur/décodeur |
| `TcpTranspac.Client` | Client TCP asynchrone basé sur TcpTranspac |
| `TcpTranspac.Server` | Serveur TCP multi‑clients avec routing |

Installation :

```bash
dotnet add package TcpTranspac.Protocol
dotnet add package TcpTranspac.Client
dotnet add package TcpTranspac.Server

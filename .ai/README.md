# Budgexa - AI Development Guide

## Project Overview
Full-stack financial management application with Angular 21 frontend and .NET 10 backend.

## General Rules
1. **Clean Code** - Follow SOLID principles
2. **Convention over Configuration** - Use established patterns
3. **DRY** - Don't Repeat Yourself
4. **Type Safety** - Leverage TypeScript and C# type systems
5. **Testing** - Backend code must keep the `tests/` suite green. Add unit tests for new domain rules, handlers, validators, behaviors and middleware before closing a change.

## Code Style
- **Indentation**: 2 spaces (TypeScript), 4 spaces (C#)
- **Naming**: 
  - camelCase for variables/methods (TS)
  - PascalCase for classes/interfaces/types
  - PascalCase for C# (all)
- **Async**: Always use async/await patterns
- **Error Handling**: Use proper try-catch and Result patterns

## Git Commit Convention
```
feat: Add new feature
fix: Bug fix
refactor: Code refactoring
docs: Documentation changes
style: Code style changes
test: Add or update tests
chore: Maintenance tasks
```

## Project Structure
```
Budgexa/
├── frontend/budgexa-client/    # Angular 22 SPA
│   └── .ai/                    # Frontend AI rules
├── backend/BudgexaApi/         # .NET 10 API (src/ + tests/)
│   ├── src/                    # Production projects (API, Application, Domain, Infrastructure)
│   ├── tests/                  # xUnit unit tests, one project per src layer
│   └── .ai/                    # Backend AI rules
└── docker-compose.yml          # Docker orchestration
```

## Before You Code
1. **Understand the context** - Read relevant .ai files
2. **Check existing code** - Don't duplicate functionality
3. **Follow patterns** - Use established project patterns
4. **Consider impact** - Think about dependencies
5. **Test your changes** - Verify functionality

## AI Assistant Tips
- Refer to `.ai/rules.md` for layer-specific guidelines
- Check `.ai/architecture.md` for structural patterns
- Ask before major architectural changes
- Propose solutions, don't assume

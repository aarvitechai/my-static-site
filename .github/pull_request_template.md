## What and why
<!-- What does this change do, and which plan.md phase/task or user story (US-xx) does it serve? -->

Phase / task: 
Stories: 

## How it was verified
<!-- Commands run, endpoints hit, screenshots for UI changes. "It builds" alone is not verification. -->

- [ ] `dotnet build` passes with 0 warnings
- [ ] `npm run build` and `npm run lint` pass (if frontend touched)
- [ ] Tests added or updated, including at least one failure case
- [ ] Manually exercised the change (describe above)

## Security checklist
- [ ] Input validated at the boundary; no entity returned directly from an endpoint
- [ ] Authorization checked (role **and** resource ownership where relevant)
- [ ] Visibility, privacy and tenant rules enforced in the SQL query, not in memory
- [ ] No secrets, tokens or personal data in code, logs or error messages
- [ ] New dependencies are necessary, maintained and free of known vulnerabilities

## Database
- [ ] No schema change, **or** migration added, reviewed, and backward compatible (expand → migrate → contract)

## Docs
- [ ] ADR added or updated if an architectural decision changed
- [ ] `plan.md` items ticked only if their DoD is met

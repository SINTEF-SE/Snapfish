
### Command pattern
The command pattern has been selected in order to achieve CQRS (https://martinfowler.com/bliki/CQRS.html)

The Command pattern is intrinsically related to the CQRS pattern that was introduced earlier in this guide. CQRS has two sides. The first area is queries, using simplified queries with the Dapper micro ORM, which was explained previously. The second area is commands, which are the starting point for transactions, and the input channel from outside the service.

        
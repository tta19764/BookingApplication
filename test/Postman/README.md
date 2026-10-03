# Postman API checks

1. Start the stack with `docker compose up --build`.
2. Import `BookingApp.Api.postman_collection.json` and `BookingApp.Local.postman_environment.json`.
3. Select the **BookingApp Local** environment.
4. Run the complete collection in its defined order.

No IDs, names, or dates need to be edited. The first request generates a unique run ID and a booking date seven days in the future. The collection verifies create, update, delete, availability search, and booking with an exact price breakdown. It can be run repeatedly against the same development database.

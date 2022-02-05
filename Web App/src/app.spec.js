import { Client } from "./app.js"

describe("A suite", function () {
    it("contains spec with an expectation", function () {
        let client = new Client();
        expect(true).toBe(true);
    });
});

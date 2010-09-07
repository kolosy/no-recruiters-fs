{
	"_id": "_design/items",
	"_rev": "9-e0831873bc49d4e61f91b9391ebbbe88",
	"language": "javascript",
	"views": {
		"alltags": {
			"map": "function (doc) {\n\t\n}"
		},
		"byShortName": {
			"map": "function (doc) {\n\tif (doc.record_type != \"posting\")\n\t\treturn;\n\n\temit(doc.shortname, null);\n}"
		},
		"byType": {
			"map": "function (doc) {\n\tif (doc.record_type != \"posting\" || !doc.published)\n\t\treturn;\n\n\temit(doc.contentType, null);\n}\n"
		},
		"byOwner": {
			"map": "function (doc) {\n\tif (doc.record_type != \"posting\")\n\t\treturn null;\n\n\temit ([doc.userId, doc.published], null)\n}"
		}
	},
	"fulltext": {
		"all": {
			"index": "function (doc) {\n\tvar ret = new Document();\n\n\tif (doc.published != true)\n\t\treturn null;\n\n\tret.add(doc.heading, {\"field\": \"title\"});\n\tret.add(doc.contents, {\"field\": \"text\"});\n\n\tif (doc.tags)\n\t\tfor (var i=0; i<doc.tags.length; i++)\n\t\t\tret.add(doc.tags[i].safeText, {\"field\": \"tag\"});\n\n\treturn ret;\n}"
		}
	}
}
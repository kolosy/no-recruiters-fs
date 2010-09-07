{
	"_id": "_design/users",
	"_rev": "5-224e33679ffaa452a2288b24d83d6d94",
	"language": "javascript",
	"views": {
		"all": {
			"map": "function (doc) {\n\tif (doc.record_type == \"user\")\n\t\temit(doc.userName, null);\t\n}"
		}
	}
}
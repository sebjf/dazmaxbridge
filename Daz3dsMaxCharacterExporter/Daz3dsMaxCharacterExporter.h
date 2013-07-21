#ifndef DAZ_CHARACTER_EXPORTER
#define DAZ_CHARACTER_EXPORTER

#include <dzscene.h>
#include "dzexporter.h"
#include "dznode.h"
#include "dzskeleton.h"
#include "dzbone.h"
#include "dzfileio.h"
#include "dzfileiosettings.h"
#include "dzobject.h"
#include "dzvertexmesh.h"
#include "dzfacetshape.h"
#include "dzfacetmesh.h"
#include "dzmaterial.h"
#include "dzbasicmaterialarea.h"
#include "dzfacegroup.h"

#include <QtCore\qfile.h>

using namespace std;

class MaxMesh; //forward declaration because we can't use Types.h because Winsock_2 is used in types (try it and see...)

/*Remember, if you make a mistake and Daz flips out it may revert to the beta workspace - change it in layout don't reinstall nothing is wrong!*/

class MyDazExporter : public DzExporter {
	Q_OBJECT
public:

	/** Constructor **/
	MyDazExporter() : DzExporter(QString("characterkit")) { }

public slots:

	virtual void    getDefaultOptions( DzFileIOSettings *options ) const;
	virtual QString getDescription() const { return QString("3DS Max Character Unwrapper"); };
	virtual bool	isFileExporter() const { return true; };

protected:

	virtual DzError	write( const QString &filename, const DzFileIOSettings *options );

private:

	MaxMesh	processMesh(DzObject* mesh);
	


};

#endif // DAZ_CHARACTER_EXPORTER
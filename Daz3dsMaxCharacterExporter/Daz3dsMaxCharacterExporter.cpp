// Daz3dsMaxCharacterExporter.cpp : Defines the exported functions for the DLL application.
//
#include "Daz3dsMaxCharacterExporter.h"




QString VecToString(DzVec3 v)
{
	return (QString::number(v.m_x) + QString(", ") + QString::number(v.m_y) + QString(", ") + QString::number(v.m_z) + QString(", ") + QString::number(v.m_w));
}

void    MyDazExporter::getDefaultOptions( DzFileIOSettings *options ) const
{
	
}

void	MyDazExporter::addMaterialData(DzShape* shape, DzShapeList shapes, MaxMesh& myMesh)
{
	for(int i = 0; i < myMesh._materialsToProcess.size(); i++)
	{
		pair<int,QString>& materialToProcess = myMesh._materialsToProcess[i];

		Material myMaterial;

		myMaterial.MaterialIndex = materialToProcess.first;
		myMaterial.MaterialName  = materialToProcess.second;

		DzMaterial* material = shape->findMaterial(materialToProcess.second);
		
		if(material == NULL)
		{
			/*geografted geometry may result in faces in one shape referencing a group where the materials are actually defined in another, so here we try to find them*/
			for(DzShapeList::Iterator itr = shapes.begin(); itr != shapes.end(); itr++)
			{
				DzMaterial* material = (*itr)->findMaterial(materialToProcess.second);
				if(material != NULL){
					break;
				}
			}
		}

		if(material == NULL)
		{
			dzApp->statusLine(QString("MyDazExporter: Unable to find material ") + QString(myMaterial.MaterialName.c_str()));
			hadErrors = true;
		}
		else
		{
			myMaterial.MaterialType = material->className();

			if(material->inherits("DzMaterial"))
			{
				myMaterial.MaterialProperties = getMaterialProperties((DzMaterial*)material);
			}
		}

		/*Even if we know the material is a dud, add it anyway and force max to deal with it, notifying the user that something has gone wrong, instead of failing silently*/
		myMesh.Materials[myMaterial.MaterialIndex] = myMaterial;
	}
}

MaxMesh MyDazExporter::getFigureMesh(DzObject* obj, DzFigure* figure)
{
	MaxMesh myMesh;

	DzFacetMesh* mesh = (DzFacetMesh*)obj->getCachedGeom();
	addGeometryData(mesh, myMesh);

	DzShape* shape = obj->getCurrentShape();
	addMaterialData(shape, getFigureShapes(getFigureFollowers(figure)), myMesh);

	return myMesh;
}

MaxMesh	MyDazExporter::getMesh(DzObject* obj)
{
	MaxMesh myMesh;

	DzFacetMesh* mesh = (DzFacetMesh*)obj->getCachedGeom();
	addGeometryData(mesh, myMesh);

	DzShape* shape = obj->getCurrentShape();
	addMaterialData(shape, DzShapeList(), myMesh);

	return myMesh;
}

void MyDazExporter::addSkinnedFigure(DzFigure* figure, DzObject* object)
{
	PreparedFigure figureInfo;

	figureInfo.figure = figure;
	figureInfo.object = object;

	DzSkeleton* parentFigure = figure->getFollowTarget();
	if(parentFigure != NULL)
	{
		return;
	}

	scene.Items.push_back( getFigureMesh(figureInfo.object, figureInfo.figure) ); //for now we dont do skinning so just export the figure

	/*the main resolve method will not iterate over bones, so we do so here checking for things like props and hair (which we also want to be parented)*/
	DzNodeList children = getSkeletonBoneChildren(figure);
	for(int i = 0; i < children.size(); i++)
	{
		resolveSelectedDzNode(children[i]);
	}
}

void MyDazExporter::addStaticMesh(DzObject* object)
{
	scene.Items.push_back( getMesh(object) );
}

void	MyDazExporter::resolveSelectedDzNode(DzNode* node)
{
	if(node->inherits("DzBone"))
	{
		//its a bone so find the parent skeleton and that will be the figure to export
		node = findBoneSkeleton(node);
	}

	DzObject* object = node->getObject();
	if(object != NULL)
	{
		//this node has geometry so its something we want to export
		if(object->getCachedGeom()->inherits("DzFacetMesh"))
		{
			if(find(processedNodes.begin(), processedNodes.end(), node) != processedNodes.end()){
				//if we have already encountered this node (which we might have done if the user has selected multiple bones in same skeleton for example), abort.
				return;
			}

			//note there might be a bug here when we traverse characters, to do with hair etc
			processedNodes.push_back(node);

			if(node->inherits("DzFigure"))
			{
				addSkinnedFigure((DzFigure*)node, object);
			}
			else
			{
				addStaticMesh(object);
			}
		}
	}

	DzNodeListIterator nodeIterator = node->nodeChildrenIterator();
	while(nodeIterator.hasNext())
	{
		DzNode* node = nodeIterator.next();
		resolveSelectedDzNode(node);		
	}
}

void	MyDazExporter::Reset()
{
	MaxScene newScene;
	scene = newScene;

	vector<DzNode*> newProcessedNodes;	
	processedNodes = newProcessedNodes;

	hadErrors = false;
}

DzError	MyDazExporter::write( const QString &filename, const DzFileIOSettings *options )
{
	Reset();

	DzNodeListIterator nodeIterator = dzScene->selectedNodeListIterator();
	while(nodeIterator.hasNext())
	{
		DzNode* node = nodeIterator.next();
		resolveSelectedDzNode(node);		
	}

	QFile myFile(filename);
	myFile.open(QIODevice::ReadWrite | QIODevice::Truncate);

	msgpack::sbuffer sbuf;
	msgpack::pack(sbuf, scene);
	myFile.write(sbuf.data(), sbuf.size());

	myFile.close();

	if(hadErrors)
	{
		dzApp->statusLine(QString("MyDazExporter: export completed with errors."));
		return DZ_UNHANDLED_EXCEPTION_ERROR;
	}
		
	return DZ_NO_ERROR;
}
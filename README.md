# Rhino3D-NonImagingOptics
Plugin for Rhino3D to create and ray trace non-imaging optical curves and surfaces

Visual studio project for a Rhino3D plugin written in C#

The plugin is written as an implementation of the plane curves described in Chapter 21 of the book "Introduction to Non Imaging Optics" 2nd edition by Julio Chaves.  

The plugin adds commands to Rhino that allow for the construction of the Non Imaging curves based on the inputs described by the corresponding curve type given in the book.  The plugin also allows for the generation of rays, the assigning of properties to the curves (reflection, refraction, or absorbing) and surfaces generated from these curves or any other surface, and the tracing of rays through the curve or surface system for a visual confirmation of the performance of a particular shape.

The intent for this plugin is to provide a quick sandbox for developing a non-imaging optical curve/surface shape to produce a desired output.  

To install the plugin from this repository copy the NonImaging.rhp and the NonImagingOptic.dll file from the bin directory into a local folder and install the plugin with the Rhino Plugin Manager.  

Future versions of the plugin are intended to provided some capability for estimating performance of the system.  
